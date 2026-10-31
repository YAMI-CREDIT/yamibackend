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
        string recipientName,
        Guid agreementId,
        string agreementTitle,
        IReadOnlyList<Installment> installments,
        string currency,
        string creatorName,
        string creditorPhone,
        string creditorName,
        string debtorPhone,
        string debtorName)
    {
        var encodedRecipientName = System.Net.WebUtility.HtmlEncode(recipientName);
        var encodedTitle = System.Net.WebUtility.HtmlEncode(agreementTitle);
        var encodedCreatorName = System.Net.WebUtility.HtmlEncode(creatorName);
        var encodedCreditorName = System.Net.WebUtility.HtmlEncode(creditorName);
        var encodedDebtorName = System.Net.WebUtility.HtmlEncode(debtorName);
        var encodedCurrency = System.Net.WebUtility.HtmlEncode(currency);
        var orderedInstallments = installments
            .OrderBy(installment => installment.dueDate)
            .ToList();
        var acceptanceDeadline = orderedInstallments[0].dueDate.ToString(
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture);
        var schedule = string.Join("<br>", orderedInstallments
            .Select((installment, index) =>
                $"Installment {index + 1}: {installment.dueDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)} - " +
                $"{installment.amountDue.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {encodedCurrency}"));

        var htmlContent =
            $"Hi {encodedRecipientName},<br>" +
            "Your agreement has been created on YAMI.<br>" +
            $"Agreement ID: {agreementId}<br>" +
            $"Title: {encodedTitle}<br>" +
            $"Accept by: {acceptanceDeadline}<br>" +
            "Installment schedule:<br>" +
            $"{schedule}<br>" +
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
