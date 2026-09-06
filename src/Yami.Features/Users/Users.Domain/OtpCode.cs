// A one-time-password issued for a login, signup verification, or password reset attempt.
// We only ever store a hash of the code, never the code itself.
public class OtpCode
{
    public Guid Id { get; set; }

    // Set when the OTP belongs to a known existing user (login, password reset).
    // Left null if we ever add pre-account-creation verification.
    public Guid? UserId { get; set; }

    // The phone number or email address the code was sent to.
    public string Destination { get; set; } = string.Empty;

    public OtpChannel Channel { get; set; }
    public OtpPurpose Purpose { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public int Attempts { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum OtpChannel
{
    Phone,
    Email
}

public enum OtpPurpose
{
    Login,
    PasswordReset
}
