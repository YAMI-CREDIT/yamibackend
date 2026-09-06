using System.ComponentModel.DataAnnotations;

/// <summary>Signup details: full name, phone number, email, and a password.</summary>
public class SignUpRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;
}
