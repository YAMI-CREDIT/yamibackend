using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/kyc")]
public class GetKycStatusController : ControllerBase
{
    private readonly IKycService _kycService;
    private readonly IUserService _userService;
    private readonly IBusinessService _businessService;

    public GetKycStatusController(
        IKycService kycService, IUserService userService, IBusinessService businessService)
    {
        _kycService = kycService;
        _userService = userService;
        _businessService = businessService;
    }

    [HttpGet("{entityId}/{entityType}")]
    [Authorize]
    [ProducesResponseType(typeof(GetKycStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(Guid entityId, string entityType)
    {
        // These come from the verified ID token's claims, not from the request.
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "Sub cannot be empty. Cannot verify the auth claim." });
        }

        if (entityType != "user" && entityType != "business")
        {
            return BadRequest(new { error = "entityType must be 'user' or 'business'." });
        }

        // Ownership check: the caller must own the entity they are querying,
        // otherwise any authenticated user could poll anyone's KYC status.
        if (entityType == "user")
        {
            var user = await _userService.GetUser(entityId);
            if (user is null)
            {
                return NotFound(new { error = "User not found." });
            }
            if (user.userSubId != sub)
            {
                return Unauthorized(new { error = "unauthorized" });
            }
        }
        else
        {
            var business = await _businessService.GetBusiness(entityId.ToString());
            if (business is null)
            {
                return NotFound(new { error = "Business not found." });
            }

            // The business belongs to the user whose id is stored on it;
            // verify that user is the caller.
            if (!Guid.TryParse(business.userId, out var ownerId))
            {
                return Unauthorized(new { error = "unauthorized" });
            }

            var owner = await _userService.GetUser(ownerId);
            if (owner is null || owner.userSubId != sub)
            {
                return Unauthorized(new { error = "unauthorized" });
            }
        }

        var verification = await _kycService.GetStatus(entityId, entityType);

        var response = new GetKycStatusResponse();

        if (verification is null)
        {
            // No row means verification was never started.
            response.verificationStatus = "NotStarted";
            response.message = "Verification has not been started.";
        }
        else
        {
            response.verificationStatus = verification.status.ToString();
            response.completedAt = verification.completedAt;

            switch (verification.status)
            {
                case VerificationStatus.InProgress:
                    response.message = "Verification in progress.";
                    break;
                case VerificationStatus.Verified:
                    response.message = "Verification completed successfully.";
                    break;
                case VerificationStatus.Failed:
                    response.message = "Verification failed.";
                    response.failureReason = verification.failureReason;
                    break;
                case VerificationStatus.Error:
                    response.message = "An error occurred during verification. Please try again.";
                    break;
            }
        }

        return Ok(response);
    }
}
