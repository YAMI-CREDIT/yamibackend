public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken);
}

// The subset of the Google ID token payload we actually care about.
public class GoogleUserInfo
{
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? FullName { get; set; }
}
