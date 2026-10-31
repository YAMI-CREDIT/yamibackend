using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agreements")]
public class UpdateAgreementController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;

    public UpdateAgreementController(
        IAgreementService agreementService,
        IUserService userService)
    {
        _agreementService = agreementService;
        _userService = userService;
    }

    [HttpPost("accept")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptAgreement([FromBody] AcceptAgreementRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        var user = await _userService.GetUserBySub(sub);

        if (user is null)
        {
            return Unauthorized(new { error = "The sub claim presented does not exist in the system." });
        }

        if (request.agreementId == Guid.Empty)
        {
            return BadRequest(new { error = "agreementId is required." });
        }

        var agreements = await _agreementService.GetAgreements(id: request.agreementId);
        var agreement = agreements.FirstOrDefault();

        if (agreement is null)
        {
            return NotFound(new { error = "Agreement not found." });
        }

        if (agreement.creditorId != user.id && agreement.debtorId != user.id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "The authenticated user is not a party to this agreement."
            });
        }

        if (agreement.createdBy == user.id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                code = "creator_cannot_accept",
                error = "The agreement creator cannot accept the agreement."
            });
        }

        if (agreement.status == AgreementStatus.Accepted)
        {
            return Conflict(new { error = "The agreement has already been accepted." });
        }

        if (agreement.creditorId == user.id)
        {
            agreement.creditorConsent = true;
            if (string.IsNullOrWhiteSpace(agreement.creditorName))
            {
                agreement.creditorName = user.name;
            }
        }
        else if (agreement.debtorId == user.id)
        {
            agreement.debtorConsent = true;
            if (string.IsNullOrWhiteSpace(agreement.debtorName))
            {
                agreement.debtorName = user.name;
            }
        }

        var acceptanceResult = await _agreementService.AcceptAgreement(agreement);
        if (acceptanceResult == AgreementAcceptanceResult.DeadlinePassed)
        {
            return Conflict(new
            {
                code = "acceptance_deadline_passed",
                error = "The acceptance deadline has passed."
            });
        }

        if (acceptanceResult == AgreementAcceptanceResult.InstallmentsUnavailable)
        {
            return Conflict(new
            {
                code = "installments_unavailable",
                error = "The agreement has no installments to accept."
            });
        }

        return Ok(new { agreementAccepted = true, agreementId = agreement.id });
    }
}