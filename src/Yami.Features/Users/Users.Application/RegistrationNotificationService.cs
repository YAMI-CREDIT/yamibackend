using Microsoft.Extensions.Logging;

public class RegistrationNotificationService : IRegistrationNotificationService
{
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegistrationNotificationService> _logger;

    public RegistrationNotificationService(
        ISmsSender smsSender,
        IEmailSender emailSender,
        ILogger<RegistrationNotificationService> logger)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendRegistrationSuccessfulSms(string phone, string userName)
    {
        var firstName = GetFirstName(userName);
        var message =
            $"Hi {firstName}\n" +
            "Thank you for registering with Yami. " +
            "Please proceed to verification of your identity. " +
            "\n" +
            "Thank you.";

        try
        {
            await _smsSender.SendSms(phone, message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send registration confirmation SMS.");
        }
    }

    public async Task SendRegistrationSuccessfulEmail(string email, string userName)
    {
        var encodedUserName = System.Net.WebUtility.HtmlEncode(userName);
        var htmlContent =
            $"Hi {encodedUserName}<br><br>" +
            "Your registration to Yami is successful.<br><br>" +
            "Please login to the App to start managing your credit records. " +
            "Complete your verification to take full advantage of the platform.<br><br>" +
            "Thank you";

        try
        {
            await _emailSender.SendEmail(
                email,
                "Welcome to Yami",
                htmlContent);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send registration confirmation email.");
        }
    }

    private static string GetFirstName(string userName) =>
        userName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
}
