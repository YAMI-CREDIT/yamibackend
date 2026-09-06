using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Asp.Versioning;

// Signup and every sign-in path: password, phone OTP, email OTP, and Google.
// AuthException thrown by IAuthService is translated to the right HTTP status by
// AuthExceptionFilter (registered in DependencyInjection.cs), so actions here stay simple.
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHostEnvironment _environment;

    public AuthController(IAuthService authService, IHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }

    /// <summary>Create an account with full name, phone number, email and password.</summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var result = await _authService.SignUpAsync(request.FullName, request.PhoneNumber, request.Email, request.Password);
        return StatusCode(StatusCodes.Status201Created, AuthResponse.From(result));
    }

    /// <summary>Sign in with Google. If no account is linked to this Google identity yet, one is
    /// created (PhoneNumber is then required) - this doubles as "sign up with Google".</summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Google([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.AuthenticateWithGoogleAsync(request.IdToken, request.PhoneNumber);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Sign in with email and password.</summary>
    [HttpPost("login/password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginWithPassword([FromBody] PasswordLoginRequest request)
    {
        var result = await _authService.LoginWithPasswordAsync(request.Email, request.Password);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Step 1 of phone login: sends a one-time code to an existing account's phone number.</summary>
    [HttpPost("login/phone/request-otp")]
    [ProducesResponseType(typeof(OtpRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestPhoneOtp([FromBody] PhoneOtpRequest request)
    {
        var code = await _authService.RequestPhoneOtpAsync(request.PhoneNumber);
        return Ok(new OtpRequestResponse
        {
            Message = "A login code was sent to your phone number.",
            DevOtp = _environment.IsProduction() ? null : code
        });
    }

    /// <summary>Step 2 of phone login: verifies the code and returns an access token.</summary>
    [HttpPost("login/phone/verify-otp")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyPhoneOtp([FromBody] VerifyPhoneOtpRequest request)
    {
        var result = await _authService.VerifyPhoneOtpAsync(request.PhoneNumber, request.Code);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Step 1 of email login: sends a one-time code to an existing account's email.</summary>
    [HttpPost("login/email/request-otp")]
    [ProducesResponseType(typeof(OtpRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestEmailOtp([FromBody] EmailOtpRequest request)
    {
        var code = await _authService.RequestEmailOtpAsync(request.Email);
        return Ok(new OtpRequestResponse
        {
            Message = "A login code was sent to your email.",
            DevOtp = _environment.IsProduction() ? null : code
        });
    }

    /// <summary>Step 2 of email login: verifies the code and returns an access token.</summary>
    [HttpPost("login/email/verify-otp")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpRequest request)
    {
        var result = await _authService.VerifyEmailOtpAsync(request.Email, request.Code);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Step 1 of password reset: sends a reset code to the account's email, if it exists.</summary>
    [HttpPost("forgot-password/request")]
    [ProducesResponseType(typeof(OtpRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var code = await _authService.RequestPasswordResetAsync(request.Email);

        // Always 200, regardless of whether the email was found - avoids leaking which emails are registered.
        return Ok(new OtpRequestResponse
        {
            Message = "If an account exists for this email, a password reset code has been sent.",
            DevOtp = _environment.IsProduction() ? null : code
        });
    }

    /// <summary>Step 2 of password reset: verifies the code and sets a new password.</summary>
    [HttpPost("forgot-password/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
        return Ok(new { message = "Password reset successfully." });
    }
}
