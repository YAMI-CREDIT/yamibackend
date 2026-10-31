public enum AgreementAcceptanceResult
{
    Accepted,
    DeadlinePassed,
    InstallmentsUnavailable
}

public sealed record AgreementCreationResult(
    Agreement agreement,
    IReadOnlyList<Installment> installments);

public interface IAgreementService
{
    Task<AgreementCreationResult?> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent, string? purpose,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan);
    Task<IReadOnlyList<Agreement>> GetAgreements(
        Guid? id = null, Guid? creditorId = null, Guid? debtorId = null);
    Task<AgreementAcceptanceResult> AcceptAgreement(Agreement agreement);
}