using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/installments")]
public class GetInstallmentsController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;

    public GetInstallmentsController(IAgreementService agreementService, IUserService userService)
    {
        _agreementService = agreementService;
        _userService = userService;
    }

    // Returns installments the authenticated user owes from today through
    // `days` days ahead, across their accepted agreements.
    [HttpGet("upcoming/owed")]
    [Authorize]
    [ProducesResponseType(typeof(GetUpcomingInstallmentsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUpcomingInstallmentsOwed([FromQuery] int days = 3)
    {
        return GetUpcomingInstallments(days, isOwed: true);
    }

    // Returns installments the authenticated user expects to receive from today
    // through `days` days ahead, across their accepted agreements.
    [HttpGet("upcoming/expected")]
    [Authorize]
    [ProducesResponseType(typeof(GetUpcomingInstallmentsResponse), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUpcomingInstallmentsExpected([FromQuery] int days = 3)
    {
        return GetUpcomingInstallments(days, isOwed: false);
    }

    [HttpGet("{installmentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Installment), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstallment(Guid installmentId)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Cannot verify the auth claim." });
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

        var installment = await _agreementService.GetInstallment(installmentId);
        if (installment is null)
        {
            return NotFound(new { error = "Installment not found." });
        }

        var agreements = await _agreementService.GetAgreements(id: installment.agreementId);
        var agreement = agreements.FirstOrDefault();
        if (agreement is null)
        {
            return NotFound(new { error = "Agreement not found." });
        }

        if (agreement.creditorId != user.id && agreement.debtorId != user.id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "The authenticated user is not a party to the agreement this installment is associated with."
            });
        }

        return Ok(installment);
    }

    private async Task<IActionResult> GetUpcomingInstallments(int days, bool isOwed)
    {
        if (days < 1)
        {
            return BadRequest(new { error = "days must be at least 1." });
        }

        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "sub cannot be empty. Cannot verify the auth claim." });
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

        var installments = isOwed
            ? await _agreementService.GetUpcomingInstallmentsOwed(user.id, days)
            : await _agreementService.GetUpcomingInstallmentsExpected(user.id, days);

        var response = new GetUpcomingInstallmentsResponse
        {
            days = days,
            direction = isOwed ? "owed" : "expected",
            installments = installments
                .Select(installment => new UpcomingInstallmentResponse
                {
                    agreementId = installment.agreementId,
                    title = installment.title,
                    currency = installment.currency,
                    dueDate = installment.dueDate,
                    amountDue = installment.amountDue
                })
                .ToList()
        };

        return Ok(response);
    }
}
