using Microsoft.EntityFrameworkCore;

public class AgreementService : IAgreementService
{
    private readonly AgreementsDbContext _db;

    public AgreementService(AgreementsDbContext db)
    {
        _db = db;
    }
    
    public async Task<AgreementCreationResult?> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent, string? purpose,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan)
    {
        if (!Enum.IsDefined(repaymentPlan))
        {
            return null;
        }

        var newAgreement = new Agreement
        {
            id = Guid.NewGuid(),
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

        var installments = InstallmentSchedule.Create(newAgreement);
        if (installments is null || installments.Count == 0)
        {
            return null;
        }

        _db.Agreements.Add(newAgreement);
        _db.Installments.AddRange(installments);
        await _db.SaveChangesAsync();

        return new AgreementCreationResult(
            newAgreement,
            installments);
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

    public async Task<AgreementAcceptanceResult> AcceptAgreement(Agreement agreement)
    {
        var installments = await _db.Installments
            .Where(i => i.agreementId == agreement.id)
            .OrderBy(i => i.dueDate)
            .ToListAsync();

        if (installments.Count == 0)
        {
            return AgreementAcceptanceResult.InstallmentsUnavailable;
        }

        var acceptedAt = DateTime.UtcNow;
        if (DateOnly.FromDateTime(acceptedAt) > installments[0].dueDate)
        {
            return AgreementAcceptanceResult.DeadlinePassed;
        }

        agreement.status = AgreementStatus.Accepted;
        agreement.acceptedAt = acceptedAt;
        foreach (var installment in installments)
        {
            installment.acceptedAt = acceptedAt;
        }

        _db.Agreements.Update(agreement);
        await _db.SaveChangesAsync();
        return AgreementAcceptanceResult.Accepted;
    }
}