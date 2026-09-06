using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Get the currently authenticated user's own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var sub = User.FindFirst("sub")?.Value;
        var user = await _userService.GetByIdAsync(Guid.Parse(sub!));

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(UserDto.From(user));
    }

    /// <summary>Get a user's profile by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInfo(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(UserDto.From(user));
    }
}
