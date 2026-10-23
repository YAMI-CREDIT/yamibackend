// Invitation notifications. Builds the invitation wording and delegates the
// actual delivery to the shared notification senders (see the Notifications
// feature), so each provider is configured and owned in one place.
public class InvitationService : IInvitationService
{
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;

    public InvitationService(ISmsSender smsSender, IEmailSender emailSender)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
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
        var htmlContent =
            $"<html><body><p>Hi, {System.Net.WebUtility.HtmlEncode(inviterName)} has invited you to join Yami.</p>" +
            "<p><a href=\"https://yami.app/register\">Click here to register</a>.</p></body></html>";

        await _emailSender.SendEmail(
            email,
            "You've been invited to join Yami",
            htmlContent);
    }
}
