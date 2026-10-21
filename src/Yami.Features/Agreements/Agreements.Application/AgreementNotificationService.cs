using Microsoft.Extensions.Logging;

public class AgreementNotificationService : IAgreementNotificationService
{
    private readonly ISmsSender _smsSender;
    private readonly ILogger<AgreementNotificationService> _logger;

    public AgreementNotificationService(
        ISmsSender smsSender,
        ILogger<AgreementNotificationService> logger)
    {
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task SendAgreementCreatedSms(
        string phone,
        string agreementTitle,
        string creatorName)
    {
        var message =
            $"{creatorName} created a new agreement with you on Yami: " +
            $"\"{agreementTitle}\". Please sign in to review and accept it.";

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
}
