// Invitation notifications. Builds the invitation wording and delegates the
// actual delivery to the shared ISmsSender (see the Notifications feature),
// so the SMS provider is configured and owned in one place.
public class InvitationService : IInvitationService
{
    private readonly ISmsSender _smsSender;

    public InvitationService(ISmsSender smsSender)
    {
        _smsSender = smsSender;
    }

    public async Task SendInvitationSms(string phone, string inviterName)
    {
        var message =
            $"Hi, {inviterName} has invited you to join Yami. " +
            "Click here to register: https://yami.app/register";

        await _smsSender.SendSms(phone, message);
    }

    public async Task SendInvitationEmail(string email, string inviterName)
    {
        // TODO: Replace with a real email provider call.
        var message =
            $"Hi, {inviterName} has invited you to join Yami. " +
            "Click here to register: https://yami.app/register";

        Console.WriteLine($"INFO: [Email stub] To {email}: {message}");

        await Task.CompletedTask;
    }
}
