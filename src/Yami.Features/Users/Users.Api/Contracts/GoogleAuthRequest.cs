using System.ComponentModel.DataAnnotations;

/// <summary>
/// Used for both "sign in with Google" and "sign up with Google": if no account is linked to
/// this Google identity yet, one is created and <see cref="PhoneNumber"/> becomes required.
/// </summary>
public class GoogleAuthRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}
