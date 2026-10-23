using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/agreements")]
public class CreateAgreementController : ControllerBase
{
    private readonly IAgreementService _agreementService;
    private readonly IUserService _userService;
    private readonly IAgreementNotificationService _agreementNotificationService;

    private static readonly Regex PhoneRegex = new(@"^\+234\d{10}$", RegexOptions.Compiled);

    public CreateAgreementController(
        IAgreementService agreementService,
        IUserService userService,
        IAgreementNotificationService agreementNotificationService)
    {
        _agreementService = agreementService;
        _userService = userService;
        _agreementNotificationService = agreementNotificationService;
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
            request.purpose,
            request.title,
            request.amountTotal,
            amountPaid: 0m,
            currency: "NGN",
            request.dueDate,
            createdBy: creator.id,
            status: AgreementStatus.Pending,
            acceptedAt: null,
            settledAt: null,
            repaymentPlan: request.repaymentPlan);

        if (newAgreement is null)
        {
            return Conflict(new { error = "The agreement could not be created." });
        }

        //Sms notification plugin to the agreement counter party
        //Still to add: email notification to both parties
        var recipient = creator.id == creditor.id ? debtor : creditor;
        await _agreementNotificationService.SendAgreementCreatedSms(
            recipient.phone,
            newAgreement.id,
            recipient.name);

        if (!string.IsNullOrWhiteSpace(recipient.email))
        {
            await _agreementNotificationService.SendAgreementCreatedEmail(
                recipient.email,
                newAgreement.id,
                newAgreement.title,
                creator.name,
                creditor.phone,
                creditor.name,
                debtor.phone,
                debtor.name);
        }

        var response = new CreateAgreementResponse
        {
            agreementCreated = true,
            agreementId = newAgreement.id
        };

        return StatusCode(201, response);
    }

    [HttpPost("invite-debtor")]
    [Authorize]
    [ProducesResponseType(typeof(CreateAgreementResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> InviteDebtor([FromBody] CreateAgreementInviteDebtorRequest request)
    {
        return await CreateAgreementWithInvitation(
            request.debtorPhone,
            request.purpose,
            request.title,
            request.amountTotal,
            request.dueDate,
            request.repaymentPlan,
            isInvitingDebtor: true);
    }

    [HttpPost("invite-creditor")]
    [Authorize]
    [ProducesResponseType(typeof(CreateAgreementResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> InviteCreditor([FromBody] CreateAgreementInviteCreditorRequest request)
    {
        return await CreateAgreementWithInvitation(
            request.creditorPhone,
            request.purpose,
            request.title,
            request.amountTotal,
            request.dueDate,
            request.repaymentPlan,
            isInvitingDebtor: false);
    }

    private async Task<IActionResult> CreateAgreementWithInvitation(
        string counterpartyPhone,
        string? purpose,
        string title,
        decimal amountTotal,
        DateOnly dueDate,
        RepaymentPlan repaymentPlan,
        bool isInvitingDebtor)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        var inviter = await _userService.GetUserBySub(sub);

        if (inviter is null)
        {
            return Unauthorized(new { error = "The sub claim presented does not exist in the system." });
        }

        var isInvitingCreditor = !isInvitingDebtor;

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new { error = "title is required" });
        }

        if (amountTotal <= 0)
        {
            return BadRequest(new { error = "amountTotal must be greater than 0." });
        }

        if (dueDate <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new { error = "dueDate must be in the future." });
        }

        if (string.IsNullOrWhiteSpace(counterpartyPhone))
        {
            return BadRequest(new { error = isInvitingDebtor ? "debtorPhone is required." : "creditorPhone is required." });
        }

        if (!PhoneRegex.IsMatch(counterpartyPhone))
        {
            return BadRequest(new { error = "phone must follow the format +234XXXXXXXXXX (e.g. +2348012345678)." });
        }

        // 4. The counterparty must not already be registered.
        var counterparty = await _userService.GetUserByPhone(counterpartyPhone);

        if (counterparty is not null)
        {
            return Conflict(new { error = "The counterparty is already registered. Use the standard create-agreement endpoint instead." });
        }

        // 5. Resolve the counterparty ID: reuse an existing pending invitation's ID
        //    if one exists for this phone, otherwise allocate a fresh one.
        var existingInvitation = await _userService.GetPendingInvitationByPhone(counterpartyPhone);
        var counterpartyId = existingInvitation?.id ?? Guid.NewGuid();

        var creditorId = isInvitingDebtor ? inviter.id : counterpartyId;
        string? creditorName = isInvitingDebtor ? inviter.name : null;
        var debtorId = isInvitingCreditor ? inviter.id : counterpartyId;
        string? debtorName = isInvitingCreditor ? inviter.name : null;

        var newAgreement = await _agreementService.CreateAgreement(
            creditorId, creditorName,
            debtorId, debtorName,
            creditorConsent: isInvitingDebtor,
            debtorConsent: isInvitingCreditor,
            purpose,
            title,
            amountTotal,
            amountPaid: 0m,
            currency: "NGN",
            dueDate,
            createdBy: inviter.id,
            status: AgreementStatus.Pending,
            acceptedAt: null,
            settledAt: null,
            repaymentPlan: repaymentPlan);

        if (newAgreement is null)
        {
            return Conflict(new { error = "The agreement could not be created." });
        }

        var invitation = await _userService.InviteToAgreement(
            counterpartyPhone,
            newAgreement.id,
            inviter.id,
            preAllocatedId: counterpartyId);

        if (invitation is null)
        {
            return Conflict(new { error = "The agreement was created, but the invitation could not be created." });
        }

        var response = new CreateAgreementResponse
        {
            agreementCreated = true,
            agreementId = newAgreement.id
        };

        return StatusCode(201, response);
    }
}
