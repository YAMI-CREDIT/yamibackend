// Provider neutral contract for sending SMS notifications. Any feature that
// needs to send an SMS depends on this and not on a specific provider.
public interface ISmsSender
{
    // Sends a plain text SMS to the given phone number. The number may be in
    // any common format (with/without +, with spaces); the implementation
    // normalizes it before calling the provider.
    Task<SendSmsResult> SendSms(string phone, string message);
}

// The provider's response, surfaced so callers can inspect the outcome.
public record SendSmsResult(string Code, string? Message);
