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
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        // get the creator's record from the sub claim
        var creator = await _userService.GetUserBySub(sub);

        if (creator is null)
        {
            return Unauthorized(new { error = "the sub claim presented does not exist in the system." });
        }

        if (string.IsNullOrWhiteSpace(request.title))
        {
            return BadRequest(new { error = "title is required" });
        }

        if (request.amountTotal <= 0)
        {
            return BadRequest(new { error = "amountTotal must be greater than 0." });
        }

        if (request.creditorId == Guid.Empty || request.debtorId == Guid.Empty)
        {
            return BadRequest(new { error = "creditorId and debtorId are required" });
        }

        if (request.dueDate <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new { error = "dueDate must be in the future." });
        }

       var creditor = await _userService.GetUser(request.creditorId);

        if (creditor is null)
        {
            return BadRequest(new { error = $"creditorId '{request.creditorId}' does not match a registered user." });
            // If we get this response on the frontend, we can trigger an invitation to yami.
        }

        var debtor = await _userService.GetUser(request.debtorId);

        if (debtor is null)
        {
            return BadRequest(new { error = $"debtorId '{request.debtorId}' does not match a registered user." });
            // If we get this response on the frontend, we can trigger an invitation to yami.
        }

        if (creditor.id == debtor.id)
        {
            return BadRequest(new { error = "The debtor cannot be the same as the creditor." });
        }

        var consentByCreditor = false;
        var consentByDebtor = false;

        if (creator.id == creditor.id)
        {
            consentByCreditor = true;
        }
        else if (creator.id == debtor.id)
        {
            consentByDebtor = true;
        }
        else
        {
            return Unauthorized(new { error = "The authenticated user is not a party to this agreement." });
        }

        var newAgreement = await _agreementService.CreateAgreement(
            creditor.id, creditor.name,
            debtor.id, debtor.name,
            creditorConsent: consentByCreditor,
            debtorConsent: consentByDebtor,
            request.title,
            request.amountTotal,
            amountPaid: 0m,
            currency: "NGN",
            request.dueDate,
            createdBy: creator.id,
            status: AgreementStatus.Pending,
            acceptedAt: null,
            settledAt: null);

        if (newAgreement is null)
        {
            return Conflict(new { error = "The agreement could not be created." });
        }

        // To add notification plug-in to send sms and email to both parties

        var response = new CreateAgreementResponse
        {
            agreementCreated = true,
            agreementId = newAgreement.id
        };

        return StatusCode(201, response);
    }
}
