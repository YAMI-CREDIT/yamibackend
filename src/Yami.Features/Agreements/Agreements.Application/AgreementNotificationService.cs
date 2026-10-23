using Microsoft.Extensions.Logging;

public class AgreementNotificationService : IAgreementNotificationService
{
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AgreementNotificationService> _logger;

    public AgreementNotificationService(
        ISmsSender smsSender,
        IEmailSender emailSender,
        ILogger<AgreementNotificationService> logger)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendAgreementCreatedSms(
        string phone,
        Guid agreementId,
        string recipientName)
    {
        var firstName = recipientName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? recipientName;
        var message =
            $"Hi {firstName},\n" +
            $"Please sign to review and accept agreement: {agreementId}\n" +
            "Thank you.";

        try
        {
            await _smsSender.SendSms(phone, message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send agreement-created SMS.");
        }
    }

    public async Task SendAgreementCreatedEmail(
        string email,
        Guid agreementId,
        string agreementTitle,
        string creatorName,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName)
    {
        var encodedTitle = System.Net.WebUtility.HtmlEncode(agreementTitle);
        var encodedCreatorName = System.Net.WebUtility.HtmlEncode(creatorName);
        var encodedCreditorName = System.Net.WebUtility.HtmlEncode(creditorName);
        var encodedDebtorName = System.Net.WebUtility.HtmlEncode(debtorName);

        var htmlContent =
            "Your agreement has been created on YAMI.<br>" +
            $"Agreement ID: {agreementId}<br>" +
            $"Title: {encodedTitle}<br>" +
            $"Creator: {encodedCreatorName}<br>" +
            $"Creditor: {System.Net.WebUtility.HtmlEncode(creditorPhone)} (Registered name: {encodedCreditorName})<br>" +
            $"Debtor: {System.Net.WebUtility.HtmlEncode(debtorPhone)} (Registered name: {encodedDebtorName})<br>" +
            "Please Login to Yami to Accept/Reject";

        try
        {
            await _emailSender.SendEmail(
                email,
                "Your agreement has been created on YAMI",
                htmlContent);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send agreement-created email.");
        }
    }
}
