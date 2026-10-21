public interface IAgreementNotificationService
{
    Task SendAgreementCreatedSms(
        string phone,
        string agreementTitle,
        string creatorName);
}
