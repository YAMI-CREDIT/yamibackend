public interface IAgreementService
{
    Task<Agreement?> CreateAgreement(
        Guid creditorId, string creditorName, Guid debtorId,
        string debtorName, string title, decimal amountTotal, DateTime dueDate);
    Task<IReadOnlyList<Agreement>> GetAgreements(
        Guid? id = null, Guid? creditorId = null, Guid? debtorId = null);
}