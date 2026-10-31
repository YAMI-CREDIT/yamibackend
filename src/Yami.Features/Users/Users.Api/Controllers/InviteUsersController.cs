using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class InviteUsersController : ControllerBase
{
    private readonly IUserService _userService;

    // Nigerian numbers in international format: +234 followed by 10 digits (e.g. +2348012345678).
    private static readonly Regex PhoneRegex = new(@"^\+234\d{10}$", RegexOptions.Compiled);

    public InviteUsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Invites a user by phone. The inviter is the
    // authenticated user; the invitee receives an SMS with a link to register.
    [HttpPost("invite/by-phone")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> InviteByPhone([FromBody] InviteByPhoneRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        var inviter = await _userService.GetUserBySub(sub);

        if (inviter is null)
        {
            return Unauthorized(new { error = "the sub claim presented does not exist in the system." });
        }

        if (string.IsNullOrWhiteSpace(request.phone))
        {
            return BadRequest(new { error = "phone is required." });
        }

        if (!PhoneRegex.IsMatch(request.phone))
        {
            return BadRequest(new { error = "phone must follow the format +234XXXXXXXXXX (e.g. +2348012345678)." });
        }

        var sent = await _userService.InviteByPhone(request.phone, inviter.id);

        if (!sent)
        {
            return BadRequest(new { error = "The invitation could not be sent." });
        }

        return StatusCode(StatusCodes.Status201Created, new { message = "Invitation sent." });
    }

    // Invites a user by email. The inviter is the
    // authenticated user; the invitee receives an email with a link to register.
    [HttpPost("invite/by-email")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> InviteByEmail([FromBody] InviteByEmailRequest request)
    {
        var sub = User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        var inviter = await _userService.GetUserBySub(sub);

        if (inviter is null)
        {
            return Unauthorized(new { error = "the sub claim presented does not exist in the system." });
        }

        if (string.IsNullOrWhiteSpace(request.email))
        {
            return BadRequest(new { error = "email is required." });
        }

        if (!Regex.IsMatch(request.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return BadRequest(new { error = "email is not a valid email address." });
        }

        var sent = await _userService.InviteByEmail(request.email, inviter.id);

        if (!sent)
        {
            return BadRequest(new { error = "The invitation could not be sent." });
        }

        return StatusCode(StatusCodes.Status201Created, new { message = "Invitation sent." });
    }
}
