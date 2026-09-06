using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

// Business-detail onboarding, collected once a user has signed up. Every route here requires
// a valid access token - the user is identified by the "sub" claim, never by a route/body id,
// so nobody can read or overwrite somebody else's onboarding record.
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/onboarding")]
[Authorize]
[Produces("application/json")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>Get the current user's onboarding/business details.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(OnboardingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var profile = await _onboardingService.GetAsync(userId);

        if (profile is null)
        {
            return NotFound(new { error = "Onboarding has not been completed yet." });
        }

        return Ok(OnboardingResponse.From(profile));
    }

    /// <summary>Create or update the current user's onboarding/business details.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(OnboardingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert([FromBody] OnboardingRequest request)
    {
        var userId = GetUserId();

        var profile = await _onboardingService.UpsertAsync(userId, new BusinessProfileInput(
            request.BusinessName,
            request.BusinessType,
            request.Industry,
            request.RegistrationNumber,
            request.BusinessAddress,
            request.BusinessEmail,
            request.BusinessPhone,
            request.YearsInOperation,
            request.EstimatedMonthlyRevenue));

        return Ok(OnboardingResponse.From(profile));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.Parse(sub!);
    }
}
