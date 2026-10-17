// Placeholder invitation implementation. Logs the message for now; swap the body
// for a real provider (e.g. Africa's Talking, Twilio) when one is chosen.
public class InvitationService : IInvitationService
{
    public async Task SendInvitationSms(string phone, string inviterName)
    {
        // TODO: Replace with a real SMS provider call.
        var message =
            $"Hi, {inviterName} has invited you to join Yami. " +
            "Click here to register: https://yami.app/register";

        Console.WriteLine($"INFO: [SMS stub] To {phone}: {message}");

        // Simulate the provider round-trip so the call site stays async.
        await Task.CompletedTask;
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
