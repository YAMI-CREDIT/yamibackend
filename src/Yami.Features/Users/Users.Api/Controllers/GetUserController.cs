using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class GetUserIdController : ControllerBase
{
    private readonly IUserService _userService;

    public GetUserIdController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserId()
    {
        // get the sub from the authenticated user's claims
        var sub = User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
        {
            return Unauthorized(new { error = "sub cannot be empty. Unable to verify the auth claim." });
        }

        var user = await _userService.GetUserBySub(sub);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        // if (user.userSubId != sub)
        // {
        //     return Unauthorized(new { error = "unauthorized" });
        // }

        var response = new GetUserResponse
        {
            message = "User retrieved successfully.",
            id = user.id
        };

        return Ok(response);
    }

    [HttpGet("by-phone")]
    [Authorize]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByPhone([FromQuery] GetUserByPhoneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.phone))
        {
            return BadRequest(new { error = "phone is required." });
        }

        var user = await _userService.GetUserByPhone(request.phone);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        var response = new GetUserResponse
        {
            message = "User retrieved successfully.",
            id = user.id
        };

        return Ok(response);
    }
}
