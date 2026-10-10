using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/kyc")]
public class VerifyEntityController : ControllerBase
{
    private readonly IKycService _kycService;
    private readonly IUserService _userService;
    private readonly IBusinessService _businessService;

    public VerifyEntityController(IKycService kycService, IUserService userService, IBusinessService businessService)
    {
        _kycService = kycService;
        _userService = userService;
        _businessService = businessService;
    }

    [HttpPost("user")]
    [Authorize]         // Ensures the authorization middleware has run before the rest of the code continues
    [ProducesResponseType(typeof(VerifyEntityResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> VerifyUser([FromBody] VerifyUserRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "Sub cannot be empty. Unable to verify the auth claim." });
        }

        var user = await _userService.GetUserBySub(sub);

        if (user is null)
        {
            return NotFound(new { error = "No user record found for the authenticated claim." });
        }

        if (user.userSubId != sub)
        {
            return Unauthorized(new { error = "The authenticated user does not match the record." });
        }

        var id = user.id;
        string identityType = request.identityType;
        string identityNumber = request.identityNumber;

        if (string.IsNullOrWhiteSpace(identityType) || string.IsNullOrWhiteSpace(identityNumber))
        {
            return BadRequest(new { error = "identityType (nin/bvn) and identityNumber are required to verify a user." });
        }

        if (identityType != "nin" && identityType != "bvn")
        {
            return BadRequest(new {
                error = $"Invalid identity type for user: {identityType}. Allowed types: 'nin', 'bvn'." });
        }


        var result = await _kycService.VerifyEntity(
            id, "user", identityType, identityNumber);

        var response = new VerifyEntityResponse
        {
            verificationStatus = result.status.ToString(),
            message = result.message
        };

        // The provider runs in the background; So this endpoint always returns 202 once verification has started.
        return Accepted(response);
    }

    [HttpPost("business")]
    [Authorize]         // Ensures the authorization middleware has run before the rest of the code continues
    [ProducesResponseType(typeof(VerifyEntityResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> VerifyBusiness([FromBody] VerifyBusinessRequest request)
    {
        // These come from the verified ID token's claims, not from the request body.
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "Sub cannot be empty. Unable to verify the auth claim." });
        }

        // Resolve the authenticated user from the sub claim.
        var user = await _userService.GetUserBySub(sub);

        if (user is null)
        {
            return NotFound(new { error = "No user record found for the authenticated claim." });
        }

        Guid businessId = request.businessId;
        string cacNumber = request.cacNumber;

        if (businessId == Guid.Empty || string.IsNullOrWhiteSpace(cacNumber))
        {
            return BadRequest(new { error = "businessId and cacNumber are required to verify a business." });
        }

        // Verify the business exists and belongs to the authenticated user.
        var business = await _businessService.GetBusiness(businessId);

        if (business is null)
        {
            return NotFound(new { error = $"Business '{businessId}' not found." });
        }

        if (business.userId != user.id)
        {
            return Unauthorized(new { error = "You are not authorized to verify this business." });
        }

        var result = await _kycService.VerifyEntity(
            businessId, "business", "cac", cacNumber);

        var response = new VerifyEntityResponse
        {
            verificationStatus = result.status.ToString(),
            message = result.message
        };

        // The provider runs in the background; So this endpoint always returns 202 once verification has started.
        return Accepted(response);
    }
}