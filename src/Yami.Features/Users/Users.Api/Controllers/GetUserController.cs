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

    public GetUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet()]
    [Authorize]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser()
    {
        // get the sub from the authenticated user's claims
        var sub = User.FindFirst("sub")?.Value;
        var user = await _userService.GetUser(sub);

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
}
