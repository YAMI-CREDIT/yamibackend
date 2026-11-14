using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/installments")]
public class UpdateInstallmentsController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;
    private readonly IAgreementNotificationService _agreementNotificationService;

    public UpdateInstallmentsController(
        IAgreementService agreementService,
        IUserService userService,
        IAgreementNotificationService agreementNotificationService)
    {
        _agreementService = agreementService;
        _userService = userService;
        _agreementNotificationService = agreementNotificationService;
    }

    [HttpPost("pay")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PayInstallment([FromBody] InstallmentPaymentRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
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

        if (request.installmentId == Guid.Empty)
        {
            return BadRequest(new { error = "installmentId is required." });
        }

        var result = await _agreementService.PayInstallment(request.installmentId, user.id);
        if (result == InstallmentPaymentResult.Paid)
        {
            var installment = await _agreementService.GetInstallment(request.installmentId);
            if (installment is null)
            {
                return NoContent();
            }

            var agreements = await _agreementService.GetAgreements(id: installment.agreementId);
            var agreement = agreements.FirstOrDefault();
            if (agreement is null)
            {
                return NoContent();
            }

            var creditor = await _userService.GetUser(agreement.creditorId);
            var creditorName = creditor?.name ?? agreement.creditorName ?? "";
            var creditorPhone = creditor?.phone ?? "";
            var debtorName = user.name;
            var debtorPhone = user.phone;

            if (creditor is not null && !string.IsNullOrWhiteSpace(creditor.email))
            {
                await _agreementNotificationService.SendInstallmentPaidEmail(
                    creditor.email,
                    creditor.name,
                    installment,
                    agreement,
                    creditorPhone,
                    creditorName,
                    debtorPhone,
                    debtorName);
            }

            if (!string.IsNullOrWhiteSpace(user.email)
                && !string.Equals(user.email, creditor?.email, StringComparison.OrdinalIgnoreCase))
            {
                await _agreementNotificationService.SendInstallmentPaidEmail(
                    user.email,
                    debtorName,
                    installment,
                    agreement,
                    creditorPhone,
                    creditorName,
                    debtorPhone,
                    debtorName);
            }

            return NoContent();
        }

        return result switch
        {
            InstallmentPaymentResult.InstallmentNotFound => NotFound(new { error = "Installment not found." }),
            InstallmentPaymentResult.AgreementNotFound => NotFound(new { error = "Agreement not found." }),
            InstallmentPaymentResult.InstallmentAlreadyPaid => Conflict(new
            {
                error = "This installment has already been paid."
            }),
            InstallmentPaymentResult.NotDebtor => StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "Only the debtor can pay this installment."
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown installment payment result.")
        };
    }

    [HttpPost("confirm-payment")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmInstallmentPayment(
        [FromBody] InstallmentPaymentRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
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

        if (request.installmentId == Guid.Empty)
        {
            return BadRequest(new { error = "installmentId is required." });
        }

        var result = await _agreementService.ConfirmInstallmentPayment(request.installmentId, user.id);
        if (result == InstallmentPaymentConfirmationResult.Confirmed)
        {
            var installment = await _agreementService.GetInstallment(request.installmentId);
            if (installment is null)
            {
                return NoContent();
            }

            var agreements = await _agreementService.GetAgreements(id: installment.agreementId);
            var agreement = agreements.FirstOrDefault();
            if (agreement is null)
            {
                return NoContent();
            }

            var debtor = await _userService.GetUser(agreement.debtorId);
            var creditorName = user.name;
            var creditorPhone = user.phone;
            var debtorName = debtor?.name ?? agreement.debtorName ?? "";
            var debtorPhone = debtor?.phone ?? "";
            var totalAmountPaid = agreement.amountPaid ?? 0m;

            if (!string.IsNullOrWhiteSpace(user.email))
            {
                await _agreementNotificationService.SendInstallmentPaymentConfirmedEmail(
                    user.email,
                    creditorName,
                    installment,
                    agreement,
                    totalAmountPaid,
                    creditorPhone,
                    creditorName,
                    debtorPhone,
                    debtorName);
            }

            if (debtor is not null
                && !string.IsNullOrWhiteSpace(debtor.email)
                && !string.Equals(debtor.email, user.email, StringComparison.OrdinalIgnoreCase))
            {
                await _agreementNotificationService.SendInstallmentPaymentConfirmedEmail(
                    debtor.email,
                    debtorName,
                    installment,
                    agreement,
                    totalAmountPaid,
                    creditorPhone,
                    creditorName,
                    debtorPhone,
                    debtorName);
            }

            return NoContent();
        }

        return result switch
        {
            InstallmentPaymentConfirmationResult.InstallmentNotFound => NotFound(new { error = "Installment not found." }),
            InstallmentPaymentConfirmationResult.AgreementNotFound => NotFound(new { error = "Agreement not found." }),
            InstallmentPaymentConfirmationResult.NotCreditor => StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "Only the creditor can confirm this payment."
            }),
            InstallmentPaymentConfirmationResult.InstallmentNotPaid => Conflict(new
            {
                error = "The debtor has not marked this installment as paid."
            }),
            InstallmentPaymentConfirmationResult.AlreadyConfirmed => Conflict(new
            {
                error = "This payment has already been confirmed."
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown installment payment confirmation result.")
        };
    }
}