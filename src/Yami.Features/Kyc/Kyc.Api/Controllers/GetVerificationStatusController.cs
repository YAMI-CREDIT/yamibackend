using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/kyc")]
public class GetVerificationStatusController : ControllerBase
{
    private readonly IKycService _kycService;
    private readonly IUserService _userService;
    private readonly IBusinessService _businessService;

    public GetVerificationStatusController(
        IKycService kycService, IUserService userService, IBusinessService businessService)
    {
        _kycService = kycService;
        _userService = userService;
        _businessService = businessService;
    }

    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(GetVerificationStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserStatus()
    {
        // These come from the verified ID token's claims, not from the request.
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

        // Ownership check: the caller must own the entity they are querying,
        // otherwise any authenticated user could poll anyone's KYC status.

        var verification = await _kycService.GetStatus(user.id, "user");

        var response = new GetVerificationStatusResponse();

        if (verification is null)
        {
            // No row means verification was never started.
            response.verificationStatus = "NotStarted";
            response.message = "Verification has not been initiated";
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

    [HttpGet("business/{businessId}")]
    [Authorize]
    [ProducesResponseType(typeof(GetVerificationStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusinessStatus(Guid businessId)
    {
        // These come from the verified ID token's claims, not from the request.
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "Sub cannot be empty. Cannot verify the auth claim." });
        }

        // Ownership check: the caller must own the business they are querying.
        var business = await _businessService.GetBusiness(businessId);
        if (business is null)
        {
            return NotFound(new { error = "Business not found." });
        }

        // The business belongs to the user whose id is stored on it;
        // verify that user is the caller.
        var ownerId = business.userId;

        var owner = await _userService.GetUser(ownerId);
        if (owner is null || owner.userSubId != sub)
        {
            return Unauthorized(new { error = "You are not authorized to view this business details" });
        }

        var verification = await _kycService.GetStatus(businessId, "business");

        var response = new GetVerificationStatusResponse();

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
