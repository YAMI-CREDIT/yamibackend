using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly UsersDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IOtpService _otpService;
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly IGoogleTokenValidator _googleTokenValidator;

    public AuthService(
        UsersDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOtpService otpService,
        ISmsSender smsSender,
        IEmailSender emailSender,
        IGoogleTokenValidator googleTokenValidator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _otpService = otpService;
        _smsSender = smsSender;
        _emailSender = emailSender;
        _googleTokenValidator = googleTokenValidator;
    }

    public async Task<AuthResult> SignUpAsync(string fullName, string phoneNumber, string? email, string password)
    {
        var phoneTaken = await _db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        if (phoneTaken)
        {
            throw new AuthException($"An account already exists for phone number '{phoneNumber}'.", 409);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
            if (emailTaken)
            {
                throw new AuthException($"An account already exists for email '{email}'.", 409);
            }
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            PasswordHash = _passwordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return IssueAuthResult(user);
    }

    public async Task<AuthResult> LoginWithPasswordAsync(string email, string password)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new AuthException("Invalid email or password.", 401);
        }

        return IssueAuthResult(user);
    }

    public async Task<string> RequestPhoneOtpAsync(string phoneNumber)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            throw new AuthException($"No account was found for phone number '{phoneNumber}'.", 404);
        }

        var code = await _otpService.IssueAsync(phoneNumber, OtpChannel.Phone, OtpPurpose.Login, user.Id);
        await _smsSender.SendAsync(phoneNumber, $"Your YamiCredit login code is {code}. It expires in 5 minutes.");
        return code;
    }

    public async Task<AuthResult> VerifyPhoneOtpAsync(string phoneNumber, string code)
    {
        await _otpService.VerifyAsync(phoneNumber, OtpChannel.Phone, OtpPurpose.Login, code);

        var user = await _db.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user is null)
        {
            throw new AuthException($"No account was found for phone number '{phoneNumber}'.", 404);
        }

        if (!user.PhoneVerified)
        {
            user.PhoneVerified = true;
            await _db.SaveChangesAsync();
        }

        return IssueAuthResult(user);
    }

    public async Task<string> RequestEmailOtpAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            throw new AuthException($"No account was found for email '{email}'.", 404);
        }

        var code = await _otpService.IssueAsync(email, OtpChannel.Email, OtpPurpose.Login, user.Id);
        await _emailSender.SendAsync(email, "Your YamiCredit login code", $"Your login code is {code}. It expires in 5 minutes.");
        return code;
    }

    public async Task<AuthResult> VerifyEmailOtpAsync(string email, string code)
    {
        await _otpService.VerifyAsync(email, OtpChannel.Email, OtpPurpose.Login, code);

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            throw new AuthException($"No account was found for email '{email}'.", 404);
        }

        if (!user.EmailVerified)
        {
            user.EmailVerified = true;
            await _db.SaveChangesAsync();
        }

        return IssueAuthResult(user);
    }

    public async Task<AuthResult> AuthenticateWithGoogleAsync(string idToken, string? phoneNumber)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(idToken);

        var user = await _db.Users.SingleOrDefaultAsync(u => u.GoogleId == googleUser.GoogleId)
            ?? await _db.Users.SingleOrDefaultAsync(u => u.Email == googleUser.Email);

        if (user is null)
        {
            // First time we've seen this Google account - this is effectively "signup with Google",
            // so we still need a phone number to complete the profile.
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new AuthException("PhoneNumber is required to complete signup with Google.", 400);
            }

            var phoneTaken = await _db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
            if (phoneTaken)
            {
                throw new AuthException($"An account already exists for phone number '{phoneNumber}'.", 409);
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                FullName = googleUser.FullName ?? googleUser.Email,
                PhoneNumber = phoneNumber,
                Email = googleUser.Email,
                EmailVerified = googleUser.EmailVerified,
                GoogleId = googleUser.GoogleId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
        }
        else if (user.GoogleId is null)
        {
            // An account already existed with this email (e.g. from phone/password signup) - link it.
            user.GoogleId = googleUser.GoogleId;
            user.EmailVerified = user.EmailVerified || googleUser.EmailVerified;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return IssueAuthResult(user);
    }

    public async Task<string?> RequestPasswordResetAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            // Do not reveal whether the email exists.
            return null;
        }

        var code = await _otpService.IssueAsync(email, OtpChannel.Email, OtpPurpose.PasswordReset, user.Id);
        await _emailSender.SendAsync(email, "Reset your YamiCredit password", $"Your password reset code is {code}. It expires in 5 minutes.");
        return code;
    }

    public async Task ResetPasswordAsync(string email, string code, string newPassword)
    {
        await _otpService.VerifyAsync(email, OtpChannel.Email, OtpPurpose.PasswordReset, code);

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            throw new AuthException($"No account was found for email '{email}'.", 404);
        }

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private AuthResult IssueAuthResult(User user)
    {
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);
        return new AuthResult(user, token, expiresAt);
    }
}
