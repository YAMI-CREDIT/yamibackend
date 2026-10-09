using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class KycService : IKycService
{
    private readonly KycDbContext _db;
    private readonly IServiceScopeFactory _scopeFactory;

    public KycService(KycDbContext db, IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _scopeFactory = scopeFactory;
    }

    public async Task<KycVerificationResult> VerifyEntity(
        Guid entityId, string entityType, string identityType, string identityNumber)
    {
        // 1. Idempotency: if a verification is already in flight for this entity,
        //    don't restart verification, just confirm it's running.
        var inFlight = await _db.KycVerifications
            .AnyAsync(k => k.entityId == entityId && k.status == VerificationStatus.InProgress);

        if (inFlight)
        {
            return new KycVerificationResult
            {
                status = VerificationStatus.InProgress,
                message = "Verification already in progress."
            };
        }

        // 2. Persist the attempt as InProgress if verification not already in Progress,
        //  so a poll right after the kyc initiation has something to read.
        var verification = new KycVerification
        {
            entityId = entityId,
            entityType = entityType,
            identityType = identityType,
            identityNumber = identityNumber,
            status = VerificationStatus.InProgress,
            createdAt = DateTime.UtcNow
        };

        _db.KycVerifications.Add(verification);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Lost a race with a concurrent request: the filtered unique index
            // rejected the insert because another InProgress row now exists.
            // Safe to treat as "already in progress".
            return new KycVerificationResult
            {
                status = VerificationStatus.InProgress,
                message = "Verification already in progress."
            };
        }

        // 3. Kick off the provider call WITHOUT awaiting it. The controller
        //    returns 202 immediately; the work continues after the response is
        //    sent. A fresh scope is required because the request scope (and its
        //    DbContexts) is disposed when the response completes.
        _ = Task.Run(() => RunVerificationAsync(
            verification.id, entityId, entityType, identityType, identityNumber));

        return new KycVerificationResult
        {
            status = VerificationStatus.InProgress,
            message = "Verification in progress."
        };
    }

    // Returns the most recent verification attempt for the entity, or null if
    // the entity has never started one (the implicit "NotStarted" state).
    public async Task<KycVerification?> GetStatus(Guid entityId, string entityType)
    {
        return await _db.KycVerifications
            .Where(k => k.entityId == entityId && k.entityType == entityType)
            .OrderByDescending(k => k.createdAt)
            .FirstOrDefaultAsync();
    }

    // Runs after the response is sent. Resolves its own scoped services from a
    // fresh scope.
    private async Task RunVerificationAsync(
        Guid verificationId, Guid entityId, string entityType, string identityType, string identityNumber)
    {
        // Scope creation is inside the try so that even a DI failure is
        // recorded as Error instead of leaving the attempt InProgress forever.
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var kycDb = scope.ServiceProvider.GetRequiredService<KycDbContext>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var businessService = scope.ServiceProvider.GetRequiredService<IBusinessService>();
            var provider = scope.ServiceProvider.GetRequiredService<IKycProvider>();

            // 4. Ask the provider.
            var result = await provider.VerifyAsync(entityType, identityType, identityNumber);

            // 5. Record the outcome on the attempt.
            var verification = await kycDb.KycVerifications.FindAsync(verificationId);
            if (verification is null)
            {
                return;
            }

            verification.status = result.status;
            if (result.status != VerificationStatus.InProgress)
            {
                verification.completedAt = DateTime.UtcNow;
            }
            if (result.status == VerificationStatus.Failed)
            {
                verification.failureReason = result.message;
            }
            await kycDb.SaveChangesAsync();

            // 6. Flip the final outcome on the entity. Each module owns its own
            //    row: users write the Users flag, businesses the Businesses flag.
            if (result.status == VerificationStatus.Verified)
            {
                if (entityType == "user")
                {
                    await userService.SetVerified(entityId, true);
                }
                else
                {
                    await businessService.SetVerified(entityId, true);
                }
            }
            else if (result.status == VerificationStatus.Failed)
            {
                if (entityType == "user")
                {
                    await userService.SetVerified(entityId, false);
                }
                else
                {
                    await businessService.SetVerified(entityId, false);
                }
            }
            // Error: no verdict was reached, so the entity's flag is left alone.
        }
        catch (Exception ex)
        {
            // A technical failure (provider down, timeout, DI, our own bug) is
            // NOT a verdict. The entity's verified flag must not change.
            // Mark the attempt Error so the client can simply retry.
            Console.WriteLine($"KYC verification for entity {entityId} errored: {ex}");

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var kycDb = scope.ServiceProvider.GetRequiredService<KycDbContext>();

                var verification = await kycDb.KycVerifications.FindAsync(verificationId);
                if (verification is not null && verification.status == VerificationStatus.InProgress)
                {
                    verification.status = VerificationStatus.Error;
                    verification.failureReason = ex.Message;
                    verification.completedAt = DateTime.UtcNow;
                    await kycDb.SaveChangesAsync();
                }
            }
            catch (Exception innerEx)
            {
                // Last resort: the DB is also unavailable. Nothing more we can do.
                Console.WriteLine($"Could not record KYC error for entity {entityId}: {innerEx.Message}");
            }
        }
    }
}
