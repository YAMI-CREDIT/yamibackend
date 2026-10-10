using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agreements")]
public class CreateAgreementController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;

    public CreateAgreementController(IAgreementService agreementService, IUserService userService)
    {
        _agreementService = agreementService;
        _userService = userService;
    }

    [HttpPost]
    [Authorize]         // Ensures the authorization middleware has run before the rest of the code continues
    [ProducesResponseType(typeof(CreateAgreementResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAgreement([FromBody] CreateAgreementRequest request)
    {
        // These come from the verified ID token's claims, not from the request body.
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "sub cannot be empty. Cannot verify the auth claim." });
        }

        // The client supplies the creditor's userId; we verify it belongs to the
        // authenticated user by matching the stored Cognito sub.
        var creditor = await _userService.GetUser(request.creditorId);

        if (creditor is null)
        {
            return BadRequest(new { error = $"creditorId '{request.creditorId}' does not match a registered user." });
        }

        if (creditor.userSubId != sub)
        {
            return Unauthorized(new { error = "unauthorized. The creditor does not match the authenticated user." });
        }

        if (string.IsNullOrWhiteSpace(request.title) || request.amountTotal <= 0)
        {
            return BadRequest(new { error = "title is required and amountTotal must be greater than zero." });
        }

        if (request.dueDate <= DateTime.UtcNow)
        {
            return BadRequest(new { error = "dueDate must be in the future." });
        }

        var debtor = await _userService.GetUser(request.debtorId);

        if (debtor is null)
        {
            return BadRequest(new { error = $"debtorId '{request.debtorId}' does not match a registered user." });
        }

        if (debtor.id == creditor.id)
        {
            return BadRequest(new { error = "The debtor cannot be the same as the creditor." });
        }

        var newAgreement = await _agreementService.CreateAgreement(
            creditor.id, creditor.name,
            debtor.id, debtor.name,
            request.title, request.amountTotal, request.dueDate);

        if (newAgreement is null)
        {
            return Conflict(new { error = "The agreement could not be created." });
        }

        var response = new CreateAgreementResponse
        {
            agreementCreated = true,
            agreementId = newAgreement.id
        };

        return StatusCode(201, response);
    }
}
