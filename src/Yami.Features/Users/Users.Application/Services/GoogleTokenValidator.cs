using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

// Verifies the ID token the client obtained from Google Sign-In (mobile/web SDK) server-side,
// so we never trust a client-asserted email/name without Google having signed it.
public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken)
    {
        var clientId = _configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is missing. Make sure it exists in appsettings.json");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new AuthException("The Google token is invalid or expired.", 401, ex);
        }

        return new GoogleUserInfo
        {
            GoogleId = payload.Subject,
            Email = payload.Email,
            EmailVerified = payload.EmailVerified,
            FullName = payload.Name
        };
    }
}
