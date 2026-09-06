// This class represents one row in the "Users" table in the database
public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneVerified { get; set; }

    // Nullable: a user can exist having only signed up with a phone number,
    // or only via Google (which always gives us an email).
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }

    // Null for accounts that only ever authenticate via OTP or Google.
    public string? PasswordHash { get; set; }

    // Set when the account is linked to a Google identity ("sub" claim from the ID token).
    public string? GoogleId { get; set; }

    public string UserType { get; set; } = "customer";
    public string? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Set once the user has submitted their onboarding/business details.
    public BusinessProfile? BusinessProfile { get; set; }
}
