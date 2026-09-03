// FYI: @Float
// This file and it's directory is added as an example to show how files should be organized.
// Please feel free to delete or replace with your own files and directories as needed or suggest a better approach

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserInfo(string id)
    {
        var user = await _userService.GetUser(id);

        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(user);
    }
}