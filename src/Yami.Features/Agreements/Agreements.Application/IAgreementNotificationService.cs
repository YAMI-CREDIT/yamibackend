public interface IAgreementNotificationService
{
    Task SendAgreementCreatedSms(
        string phone,
        Guid agreementId,
        string recipientName);

    Task SendAgreementCreatedEmail(
        string email,
        string recipientName,
        Guid agreementId,
        string agreementTitle,
        IReadOnlyList<Installment> installments,
        string currency,
        string creatorName,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName);
}
