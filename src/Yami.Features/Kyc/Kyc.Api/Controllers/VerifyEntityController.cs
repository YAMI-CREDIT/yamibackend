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

    public VerifyEntityController(IKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost]
    [Authorize]         // Ensures the authorization middleware has run before the rest of the code continues
    [ProducesResponseType(typeof(VerifyEntityResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> VerifyEntity([FromBody] VerifyEntityRequest request)
    {
        // These come from the verified ID token's claims, not from the request body.
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "Sub cannot be empty. Cannot verify the auth claim." });
        }

        string entityId = request.entityId;
        string identityType = request.identityType;
        string identityNumber = request.identityNumber;
        string entityType = request.entityType;

        if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(identityType)
            || string.IsNullOrWhiteSpace(identityNumber) || string.IsNullOrWhiteSpace(entityType))
        {
            return BadRequest(new { error = "entityId, identityType, identityNumber and entityType are required." });
        }

        if (entityType == "user" && identityType != "nin" && identityType != "bvn")
        {
            return BadRequest(new {
                error = $"Invalid identity type for user: {identityType}. Allowed types: 'nin', 'bvn'." });
        }

        if (entityType == "business" && identityType != "cac")
        {
            return BadRequest(new { error = $"Invalid identity type for business: {identityType}. Allowed types: 'cac'." });
        }

        if (!Guid.TryParse(entityId, out var entityIdGuid))
        {
            return BadRequest(new { error = "entityId must be a valid GUID." });
        }

        var result = await _kycService.VerifyEntity(
            entityIdGuid, entityType, identityType, identityNumber);

        var response = new VerifyEntityResponse
        {
            verificationStatus = result.status.ToString(),
            message = result.message
        };

        // The provider runs in the background; So this endpoint always returns 202 once verification has started.
        return Accepted(response);
    }
}