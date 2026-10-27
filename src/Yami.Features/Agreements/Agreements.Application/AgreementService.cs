using Microsoft.EntityFrameworkCore;

public class AgreementService : IAgreementService
{
    private readonly AgreementsDbContext _db;

    public AgreementService(AgreementsDbContext db)
    {
        _db = db;
    }
    
    // this function is a placeholder to create a new agreement.
    // The controller will be implemented later
    public async Task<Agreement?> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent, string? purpose,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan)
    {
        var newAgreement = new Agreement
        {
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
        await _db.SaveChangesAsync();

        return newAgreement;
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

    public async Task UpdateAgreement(Agreement agreement)
    {
        _db.Agreements.Update(agreement);
        await _db.SaveChangesAsync();
    }
}