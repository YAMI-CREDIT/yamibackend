// This interface is a CONTRACT covering every way a user can sign up or sign in:
// full name/phone/email + password, phone OTP, email OTP, and Google.
public interface IAuthService
{
    Task<AuthResult> SignUpAsync(string fullName, string phoneNumber, string? email, string password);

    Task<AuthResult> LoginWithPasswordAsync(string email, string password);

    /// <summary>Sends a login OTP to an existing user's phone number. Returns the plaintext code
    /// (callers may echo it back in non-production environments to ease testing).</summary>
    Task<string> RequestPhoneOtpAsync(string phoneNumber);
    Task<AuthResult> VerifyPhoneOtpAsync(string phoneNumber, string code);

    /// <summary>Sends a login OTP to an existing user's email. Returns the plaintext code
    /// (callers may echo it back in non-production environments to ease testing).</summary>
    Task<string> RequestEmailOtpAsync(string email);
    Task<AuthResult> VerifyEmailOtpAsync(string email, string code);

    /// <summary>Signs in an existing Google-linked/email-matching user, or creates a new
    /// account when neither match (in which case <paramref name="phoneNumber"/> is required).</summary>
    Task<AuthResult> AuthenticateWithGoogleAsync(string idToken, string? phoneNumber);

    /// <summary>Issues a password-reset OTP if the email is registered. Never reveals whether
    /// the email exists; returns the plaintext code (for non-production echoing) or null when
    /// there was nothing to send.</summary>
    Task<string?> RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string email, string code, string newPassword);
}
