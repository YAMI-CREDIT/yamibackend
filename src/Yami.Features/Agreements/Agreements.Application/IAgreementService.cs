public enum AgreementAcceptanceResult
{
    Accepted,
    DeadlinePassed,
    InstallmentsUnavailable
}

public sealed record UpcomingInstallment(
    Guid agreementId,
    string title,
    string currency,
    DateOnly dueDate,
    decimal amountDue);

public interface IAgreementService
{
    Task<Agreement?> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent, string? purpose,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan);
    Task<IReadOnlyList<Agreement>> GetAgreements(
        Guid? id = null, Guid? creditorId = null, Guid? debtorId = null);
    Task<IReadOnlyList<Installment>> GetInstallments(Guid agreementId);
    Task<IReadOnlyList<UpcomingInstallment>> GetUpcomingInstallmentsOwed(Guid userId, int days);
    Task<IReadOnlyList<UpcomingInstallment>> GetUpcomingInstallmentsExpected(Guid userId, int days);
    Task<AgreementAcceptanceResult> AcceptAgreement(Agreement agreement);
}