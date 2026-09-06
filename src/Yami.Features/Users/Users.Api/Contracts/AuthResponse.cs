/// <summary>Returned by every signup/login endpoint: the access token alongside the user profile.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;

    public static AuthResponse From(AuthResult result) => new()
    {
        Token = result.Token,
        ExpiresAt = result.ExpiresAt,
        User = UserDto.From(result.User)
    };
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneVerified { get; set; }
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public string UserType { get; set; } = string.Empty;
    public bool HasCompletedOnboarding { get; set; }

    public static UserDto From(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        PhoneVerified = user.PhoneVerified,
        Email = user.Email,
        EmailVerified = user.EmailVerified,
        UserType = user.UserType,
        HasCompletedOnboarding = user.BusinessProfile?.IsCompleted ?? false
    };
}

/// <summary>Acknowledgement for endpoints that only send an OTP (no token yet).
/// <see cref="DevOtp"/> is only populated outside the Production environment, to make manual
/// testing possible without a real SMS/email provider wired up.</summary>
public class OtpRequestResponse
{
    public string Message { get; set; } = string.Empty;
    public string? DevOtp { get; set; }
}
