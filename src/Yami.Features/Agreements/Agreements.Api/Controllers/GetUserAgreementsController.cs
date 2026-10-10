using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agreements")]
public class GetUserAgreementsController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;

    public GetUserAgreementsController(IAgreementService agreementService, IUserService userService)
    {
        _agreementService = agreementService;
        _userService = userService;
    }

    // Returns the user's debts (as debtor) and assets (as creditor).
    // The user must be the authenticated user, verified via the Cognito sub.
    [HttpGet("user/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(GetUserAgreementsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAgreements(Guid id)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "sub cannot be empty. Cannot verify the auth claim." });
        }

        var user = await _userService.GetUser(id);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        if (user.userSubId != sub)
        {
            return Unauthorized(new { error = "unauthorized" });
        }

        var assets = await _agreementService.GetAgreements(creditorId: id);
        var debts = await _agreementService.GetAgreements(debtorId: id);

        var response = new GetUserAgreementsResponse
        {
            assets = assets ?? new List<Agreement>(),
            debts = debts ?? new List<Agreement>()
        };

        return Ok(response);
    }
}
