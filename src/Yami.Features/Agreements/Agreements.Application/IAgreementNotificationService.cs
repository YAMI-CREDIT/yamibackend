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
        string creatorName,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName,
        DateTime acceptBy);

    Task SendAgreementAcceptedEmail(
        string email,
        string recipientName,
        Guid agreementId,
        string agreementTitle,
        DateTime acceptedAt,
        IReadOnlyList<Installment> installments,
        string currency,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName);

    Task SendInstallmentPaidEmail(
        string email,
        string recipientName,
        Installment installment,
        Agreement agreement,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName);

    Task SendInstallmentPaymentConfirmedEmail(
        string email,
        string recipientName,
        Installment installment,
        Agreement agreement,
        decimal totalAmountPaid,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName);
}
