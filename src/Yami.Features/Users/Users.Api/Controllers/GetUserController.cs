using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class GetUserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAgreementService _agreementService;

    public GetUserController(IUserService userService, IAgreementService agreementService)
    {
        _userService = userService;
        _agreementService = agreementService;
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo(Guid id)
    {
        // get the sub from the authenticated user's claims
        var sub = User.FindFirst("sub")?.Value;
        var user = await _userService.GetUser(id);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        if (user.userSubId != sub)
        {
            return Unauthorized(new { error = "unauthorized" });
        }

        var userAssets = await _agreementService.GetAgreements(creditorId: id);

        if (userAssets is null)
        {
            return BadRequest(new { error = "agreement id, creditor id or debtor id is required." });
        }

        var userDebts = await _agreementService.GetAgreements(debtorId: id);

        if (userDebts is null)
        {
            return BadRequest(new { error = "agreement id, creditor id or debtor id is required." });
        }

        var response = new GetUserResponse
        {
            name = user.name,
            debts = userDebts,
            assets = userAssets
        };

        return Ok(response);
    }
}