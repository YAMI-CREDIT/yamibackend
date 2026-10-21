using Microsoft.Extensions.Logging;

public class RegistrationNotificationService : IRegistrationNotificationService
{
    private readonly ISmsSender _smsSender;
    private readonly ILogger<RegistrationNotificationService> _logger;

    public RegistrationNotificationService(
        ISmsSender smsSender,
        ILogger<RegistrationNotificationService> logger)
    {
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task SendRegistrationSuccessfulSms(string phone)
    {
        const string message =
            "Thank you for registering with Yami. " +
            "Please proceed to verification of your identity " +
            "to take full advantage of the platform." +
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
}
