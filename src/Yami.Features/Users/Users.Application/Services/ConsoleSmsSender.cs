// DEV STUB: logs the message instead of sending a real SMS.
// Swap this registration in DependencyInjection.cs for a real provider
// (e.g. Twilio, Termii, AWS SNS) before going to production.
public class ConsoleSmsSender : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message)
    {
        Console.WriteLine($"[SMS -> {phoneNumber}] {message}");
        return Task.CompletedTask;
    }
}
