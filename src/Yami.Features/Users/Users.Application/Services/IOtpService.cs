// Generates, stores (hashed) and verifies one-time-passwords. Delivery (SMS/email) is the
// caller's job - this service only owns the code's lifecycle.
public interface IOtpService
{
    /// <summary>Creates and stores a new OTP, returning the plaintext code to send/echo.</summary>
    Task<string> IssueAsync(string destination, OtpChannel channel, OtpPurpose purpose, Guid? userId);

    /// <summary>Verifies and consumes the most recent pending OTP for this destination/purpose.
    /// Throws <see cref="AuthException"/> when there is no pending code, it expired, it was
    /// guessed wrong too many times, or the code itself doesn't match.</summary>
    Task VerifyAsync(string destination, OtpChannel channel, OtpPurpose purpose, string code);
}
