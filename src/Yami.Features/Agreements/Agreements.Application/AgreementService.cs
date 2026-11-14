using Microsoft.EntityFrameworkCore;

public class AgreementService : IAgreementService
{
    public const int AcceptanceWindowHours = 48;

    private readonly AgreementsDbContext _db;

    public AgreementService(AgreementsDbContext db)
    {
        _db = db;
    }
    
    public async Task<AgreementCreationResult> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent, string? purpose,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan,
        Guid idempotencyKey)
    {
        if (!Enum.IsDefined(repaymentPlan))
        {
            return new(AgreementCreationStatus.InvalidRepaymentPlan, null);
        }

        var existingKey = await _db.Agreements
            .FirstOrDefaultAsync(item => item.createdBy == createdBy
                && item.idempotencyKey == idempotencyKey);
        if (existingKey is not null)
        {
            return new(AgreementCreationStatus.IdempotencyKeyConflict, null);
        }

        var newAgreement = new Agreement
        {
            id = Guid.NewGuid(),
            idempotencyKey = idempotencyKey,
            creditorId = creditorId,
            creditorName = creditorName,
            debtorId = debtorId,
            debtorName = debtorName,
            creditorConsent = creditorConsent,
            debtorConsent = debtorConsent,
            purpose = purpose,
            title = title,
            amountTotal = amountTotal,
            amountPaid = amountPaid,
            currency = currency,
            dueDate = dueDate,
            createdAt = DateTime.UtcNow,
            createdBy = createdBy,
            status = status,
            acceptedAt = acceptedAt,
            settledAt = settledAt,
            repaymentPlan = repaymentPlan
        };

        _db.Agreements.Add(newAgreement);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.Entry(newAgreement).State = EntityState.Detached;
            var concurrentAgreement = await _db.Agreements
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.createdBy == createdBy
                    && item.idempotencyKey == idempotencyKey);
            if (concurrentAgreement is null)
            {
                throw;
            }

            return new(AgreementCreationStatus.IdempotencyKeyConflict, null);
        }

        return new(AgreementCreationStatus.Created, newAgreement);
    }
    public async Task<IReadOnlyList<Agreement>> GetAgreements(
        Guid? id = null, Guid? creditorId = null, Guid? debtorId = null)
    {
        if (id == null && creditorId == null && debtorId == null)
        {
            return [];
        }

        var query = _db.Agreements.AsQueryable();

        if (id.HasValue)
        {
            query = query.Where(a => a.id == id.Value);
        }

        if (creditorId.HasValue)
        {
            query = query.Where(a => a.creditorId == creditorId.Value);
        }

        if (debtorId.HasValue)
        {
            query = query.Where(a => a.debtorId == debtorId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<Installment>> GetInstallments(Guid agreementId)
    {
        return await _db.Installments
            .Where(installment => installment.agreementId == agreementId)
            .OrderBy(installment => installment.dueDate)
            .ToListAsync();
    }

    public Task<Installment?> GetInstallment(Guid installmentId)
    {
        return _db.Installments
            .FirstOrDefaultAsync(installment => installment.id == installmentId);
    }

    public Task<IReadOnlyList<UpcomingInstallment>> GetUpcomingInstallmentsOwed(Guid userId, int days)
    {
        return GetUpcomingInstallments(userId, days, isDebtor: true);
    }

    public Task<IReadOnlyList<UpcomingInstallment>> GetUpcomingInstallmentsExpected(Guid userId, int days)
    {
        return GetUpcomingInstallments(userId, days, isDebtor: false);
    }

    private async Task<IReadOnlyList<UpcomingInstallment>> GetUpcomingInstallments(
        Guid userId, int days, bool isDebtor)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = today.AddDays(days);

        var query = from installment in _db.Installments
                    join agreement in _db.Agreements on installment.agreementId equals agreement.id
                    where agreement.status == AgreementStatus.Accepted
                        && installment.dueDate >= today
                        && installment.dueDate <= end
                    select new { installment, agreement };

        query = isDebtor
            ? query.Where(item => item.agreement.debtorId == userId)
            : query.Where(item => item.agreement.creditorId == userId);

        return await query
            .OrderBy(item => item.installment.dueDate)
            .Select(item => new UpcomingInstallment(
                item.agreement.id,
                item.agreement.title,
                item.agreement.currency,
                item.installment.dueDate,
                item.installment.amountDue))
            .ToListAsync();
    }

    public async Task<AgreementAcceptanceResult> AcceptAgreement(Agreement agreement)
    {
        var acceptedAt = DateTime.UtcNow;
        if (acceptedAt > agreement.createdAt.AddHours(AcceptanceWindowHours))
        {
            return AgreementAcceptanceResult.DeadlinePassed;
        }

        var acceptedInstallments = InstallmentSchedule.Create(
            agreement,
            DateOnly.FromDateTime(acceptedAt));
        if (acceptedInstallments is null || acceptedInstallments.Count == 0)
        {
            return AgreementAcceptanceResult.InstallmentsUnavailable;
        }

        foreach (var installment in acceptedInstallments)
        {
            installment.acceptedAt = acceptedAt;
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var updatedAgreements = await _db.Agreements
            .Where(item => item.id == agreement.id && item.status == AgreementStatus.Pending)
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.status, AgreementStatus.Accepted)
                .SetProperty(item => item.acceptedAt, acceptedAt)
                .SetProperty(item => item.creditorConsent, agreement.creditorConsent)
                .SetProperty(item => item.debtorConsent, agreement.debtorConsent)
                .SetProperty(item => item.creditorName, agreement.creditorName)
                .SetProperty(item => item.debtorName, agreement.debtorName));

        if (updatedAgreements == 0)
        {
            await transaction.RollbackAsync();
            return AgreementAcceptanceResult.NotPending;
        }

        agreement.status = AgreementStatus.Accepted;
        agreement.acceptedAt = acceptedAt;
        _db.Entry(agreement).State = EntityState.Detached;
        _db.Installments.AddRange(acceptedInstallments);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();
        return AgreementAcceptanceResult.Accepted;
    }

    public async Task<InstallmentPaymentResult> PayInstallment(Guid installmentId, Guid debtorId)
    {
        var installment = await _db.Installments
            .FirstOrDefaultAsync(item => item.id == installmentId);
        if (installment is null)
        {
            return InstallmentPaymentResult.InstallmentNotFound;
        }

        var agreement = await _db.Agreements
            .FirstOrDefaultAsync(item => item.id == installment.agreementId);
        if (agreement is null)
        {
            return InstallmentPaymentResult.AgreementNotFound;
        }

        if (agreement.debtorId != debtorId)
        {
            return InstallmentPaymentResult.NotDebtor;
        }

        if (installment.debtorPaid)
        {
            return InstallmentPaymentResult.InstallmentAlreadyPaid;
        }

        var debtorPaidAt = DateTime.UtcNow;
        installment.debtorPaid = true;
        installment.debtorPaidAt = debtorPaidAt;

        await _db.SaveChangesAsync();
        return InstallmentPaymentResult.Paid;
    }

    public async Task<InstallmentPaymentConfirmationResult> ConfirmInstallmentPayment(
        Guid installmentId, Guid creditorId)
    {
        var installment = await _db.Installments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id == installmentId);
        if (installment is null)
        {
            return InstallmentPaymentConfirmationResult.InstallmentNotFound;
        }

        var agreement = await _db.Agreements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id == installment.agreementId);
        if (agreement is null)
        {
            return InstallmentPaymentConfirmationResult.AgreementNotFound;
        }

        if (agreement.creditorId != creditorId)
        {
            return InstallmentPaymentConfirmationResult.NotCreditor;
        }

        if (installment.creditorConfirmedPayment)
        {
            return InstallmentPaymentConfirmationResult.AlreadyConfirmed;
        }

        if (!installment.debtorPaid)
        {
            return InstallmentPaymentConfirmationResult.InstallmentNotPaid;
        }

        var confirmedAt = DateTime.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var updatedInstallments = await _db.Installments
            .Where(item => item.id == installmentId
                && item.agreementId == agreement.id
                && item.debtorPaid
                && !item.creditorConfirmedPayment)
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.creditorConfirmedPayment, true)
                .SetProperty(item => item.creditorConfirmedPaymentAt, confirmedAt));

        if (updatedInstallments == 0)
        {
            await transaction.RollbackAsync();
            var currentInstallment = await _db.Installments
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.id == installmentId);
            if (currentInstallment is null)
            {
                return InstallmentPaymentConfirmationResult.InstallmentNotFound;
            }

            return currentInstallment.creditorConfirmedPayment
                ? InstallmentPaymentConfirmationResult.AlreadyConfirmed
                : InstallmentPaymentConfirmationResult.InstallmentNotPaid;
        }

        var updatedAgreements = await _db.Agreements
            .Where(item => item.id == agreement.id)
            .ExecuteUpdateAsync(update => update.SetProperty(
                item => item.amountPaid,
                item => (item.amountPaid ?? 0m) + installment.amountDue));
        if (updatedAgreements == 0)
        {
            await transaction.RollbackAsync();
            return InstallmentPaymentConfirmationResult.AgreementNotFound;
        }

        await transaction.CommitAsync();
        return InstallmentPaymentConfirmationResult.Confirmed;
    }

    public async Task<InstallmentUnpaymentResult> UnpayInstallment(Guid installmentId, Guid debtorId)
    {
        var installment = await _db.Installments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id == installmentId);
        if (installment is null)
        {
            return InstallmentUnpaymentResult.InstallmentNotFound;
        }

        var agreement = await _db.Agreements
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id == installment.agreementId);
        if (agreement is null)
        {
            return InstallmentUnpaymentResult.AgreementNotFound;
        }

        if (agreement.debtorId != debtorId)
        {
            return InstallmentUnpaymentResult.NotDebtor;
        }

        if (installment.creditorConfirmedPayment)
        {
            return InstallmentUnpaymentResult.AlreadyConfirmed;
        }

        if (!installment.debtorPaid)
        {
            return InstallmentUnpaymentResult.AlreadyUnpaid;
        }

        var updatedInstallments = await _db.Installments
            .Where(item => item.id == installmentId
                && item.agreementId == agreement.id
                && item.debtorPaid
                && !item.creditorConfirmedPayment)
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.debtorPaid, false)
                .SetProperty(item => item.debtorPaidAt, (DateTime?)null));

        if (updatedInstallments > 0)
        {
            return InstallmentUnpaymentResult.Unpaid;
        }

        var currentInstallment = await _db.Installments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id == installmentId);
        if (currentInstallment is null)
        {
            return InstallmentUnpaymentResult.InstallmentNotFound;
        }

        return currentInstallment.creditorConfirmedPayment
            ? InstallmentUnpaymentResult.AlreadyConfirmed
            : InstallmentUnpaymentResult.AlreadyUnpaid;
    }
}