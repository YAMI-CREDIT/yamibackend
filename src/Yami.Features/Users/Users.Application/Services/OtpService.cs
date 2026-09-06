using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class OtpService : IOtpService
{
    private const int CodeLength = 6;
    private const int ExpiryMinutes = 5;
    private const int MaxAttempts = 5;

    // Minimum gap before a fresh OTP can be requested again for the same destination/purpose,
    // so a client can't hammer the SMS/email provider by re-requesting in a loop.
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(45);

    private readonly UsersDbContext _db;

    public OtpService(UsersDbContext db)
    {
        _db = db;
    }

    public async Task<string> IssueAsync(string destination, OtpChannel channel, OtpPurpose purpose, Guid? userId)
    {
        var mostRecent = await _db.OtpCodes
            .Where(o => o.Destination == destination && o.Purpose == purpose)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (mostRecent is not null && mostRecent.ConsumedAt is null
            && DateTime.UtcNow - mostRecent.CreatedAt < ResendCooldown)
        {
            throw new AuthException("Please wait a moment before requesting another code.", 429);
        }

        var code = GenerateNumericCode(CodeLength);

        _db.OtpCodes.Add(new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Destination = destination,
            Channel = channel,
            Purpose = purpose,
            CodeHash = Hash(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return code;
    }

    public async Task VerifyAsync(string destination, OtpChannel channel, OtpPurpose purpose, string code)
    {
        var otp = await _db.OtpCodes
            .Where(o => o.Destination == destination && o.Channel == channel
                && o.Purpose == purpose && o.ConsumedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
        {
            throw new AuthException("No pending code was found for this destination. Please request a new one.", 400);
        }

        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            throw new AuthException("This code has expired. Please request a new one.", 400);
        }

        if (otp.Attempts >= MaxAttempts)
        {
            throw new AuthException("Too many incorrect attempts. Please request a new code.", 400);
        }

        if (otp.CodeHash != Hash(code))
        {
            otp.Attempts += 1;
            await _db.SaveChangesAsync();
            throw new AuthException("The code you entered is incorrect.", 400);
        }

        otp.ConsumedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static string GenerateNumericCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = (char)('0' + bytes[i] % 10);
        }

        return new string(chars);
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
