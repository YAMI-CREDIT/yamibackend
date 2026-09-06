// DEV STUB: logs the message instead of sending a real email.
// Swap this registration in DependencyInjection.cs for a real provider
// (e.g. SendGrid, AWS SES, SMTP) before going to production.
public class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string email, string subject, string message)
    {
        Console.WriteLine($"[Email -> {email}] {subject}: {message}");
        return Task.CompletedTask;
    }
}
