public interface IAgreementService
{
    Task<Agreement?> CreateAgreement(
        Guid creditorId, string? creditorName, Guid debtorId,
        string? debtorName, bool creditorConsent, bool debtorConsent,
        string title, decimal amountTotal, decimal? amountPaid, string currency,
        DateOnly dueDate, Guid createdBy, AgreementStatus status,
        DateTime? acceptedAt, DateTime? settledAt, RepaymentPlan repaymentPlan);
    Task<IReadOnlyList<Agreement>> GetAgreements(
        Guid? id = null, Guid? creditorId = null, Guid? debtorId = null);
}