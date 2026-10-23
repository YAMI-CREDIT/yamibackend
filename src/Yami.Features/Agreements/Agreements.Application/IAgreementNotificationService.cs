public interface IAgreementNotificationService
{
    Task SendAgreementCreatedSms(
        string phone,
        Guid agreementId,
        string recipientName);

    Task SendAgreementCreatedEmail(
        string email,
        Guid agreementId,
        string agreementTitle,
        string creatorName,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName);
}
