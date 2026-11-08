using Microsoft.EntityFrameworkCore;

public class AgreementService : IAgreementService
{
    public const int AcceptanceWindowHours = 48;

    private readonly AgreementsDbContext _db;

    public AgreementService(AgreementsDbContext db)
    {
        _db = db;
    }
    
    public async Task<Agreement?> CreateAgreement(
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

    public async Task<IReadOnlyList<Installment>> GetInstallments(Guid agreementId)
    {
        return await _db.Installments
            .Where(installment => installment.agreementId == agreementId)
            .OrderBy(installment => installment.dueDate)
            .ToListAsync();
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

        agreement.status = AgreementStatus.Accepted;
        agreement.acceptedAt = acceptedAt;
        foreach (var installment in acceptedInstallments)
        {
            installment.acceptedAt = acceptedAt;
        }

        _db.Installments.AddRange(acceptedInstallments);
        _db.Agreements.Update(agreement);
        await _db.SaveChangesAsync();
        return AgreementAcceptanceResult.Accepted;
    }
}