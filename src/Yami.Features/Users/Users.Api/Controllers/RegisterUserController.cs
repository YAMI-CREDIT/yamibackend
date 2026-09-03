using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class RegisterUserController : ControllerBase
{
    private readonly IUserService _userService;

    public RegisterUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        string phone = request.phone;
        string name = request.name;
        string userType = request.userType;
        string dateOfBirth = request.dateOfBirth;

        if (string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(userType) ||
            string.IsNullOrWhiteSpace(dateOfBirth))
        {
            return BadRequest(new { error = "phone, name , userType and dateOfBirth are required." });
        }

        var newUser = await _userService.RegisterUser(phone, name, userType, dateOfBirth);

        // I would not expect us to fufil this condition as cognito should not allow
        // the registration in the first place.
        if (newUser is null)
        {
            return Conflict(new { error = "This user is already registered." });
        }

        return Ok(new { id = newUser.id, phone = newUser.phone, name = newUser.name });
    }
}