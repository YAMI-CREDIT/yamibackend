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
    [HttpGet()]
    [Authorize]
    [ProducesResponseType(typeof(GetUserAgreementsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAgreements()
    {
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

        var id = user.id;

        var assets = await _agreementService.GetAgreements(creditorId: id);
        var debts = await _agreementService.GetAgreements(debtorId: id);

        var response = new GetUserAgreementsResponse
        {
            assets = assets ?? new List<Agreement>(),
            debts = debts ?? new List<Agreement>()
        };

        return Ok(response);
    }

    [HttpGet("{agreementId:guid}/installments")]
    [Authorize]
    [ProducesResponseType(typeof(GetAgreementInstallmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgreementInstallments(Guid agreementId)
    {
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

        var agreements = await _agreementService.GetAgreements(id: agreementId);
        var agreement = agreements.FirstOrDefault();

        if (agreement is null)
        {
            return NotFound(new { error = "Agreement not found." });
        }

        if (agreement.creditorId != user.id && agreement.debtorId != user.id)
        {
            return Unauthorized(new { error = "The authenticated user is not a party to this agreement." });
        }

        var installments = await _agreementService.GetInstallments(agreementId);
        var response = new GetAgreementInstallmentsResponse
        {
            agreementId = agreementId,
            installments = installments
                .Select(installment => new AgreementInstallmentResponse
                {
                    dueDate = installment.dueDate,
                    amountDue = installment.amountDue
                })
                .ToList()
        };

        return Ok(response);
    }
}
