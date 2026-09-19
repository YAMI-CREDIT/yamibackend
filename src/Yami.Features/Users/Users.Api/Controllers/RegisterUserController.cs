using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class RegisterUserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBusinessService _businessService;

    public RegisterUserController(IUserService userService, IBusinessService businessService)
    {
        _userService = userService;
        _businessService = businessService;
    }
    
    [HttpPost]
    [Authorize]         // This ensure ths authorization middleware has run before allowing the rest of the code to continue
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {

        // DEBUG: To see every item returned from cognito during authentication verification    
        // foreach (var claim in User.Claims)
        // {
        //     Console.WriteLine($"{claim.Type} = {claim.Value}");
        // }

        // These come from the verified ID token's claims, not from the request body.
        var sub = User.FindFirst("sub")?.Value;
        var phoneNumber = User.FindFirst("phone_number")?.Value;

        if (sub is null)
        {
            return Unauthorized(new { error = "sub cannot be empty" });
        }

        if (phoneNumber != request.phone)
        {
            return  Unauthorized(new { error = "invalid credentials. Unable to validate phone number" });
        }

        string phone = request.phone;
        string name = request.name;
        string? email = request.email;
        bool termsAccepted = true;
        string? businessName = null;
        string? cacNo = null;
        string? area = null;
        string? identityType = null;
        string? identityNumber = null;
        string? userType = null;
        string? dateOfBirth = request.dateOfBirth;

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "phone number and name are required." });
        }


        var newUser = await _userService.RegisterUser(
            phone, name, sub, dateOfBirth, email,
            termsAccepted, area, identityType,
            identityNumber, userType);

        // I would not expect us to fufil this condition as cognito should not allow
        // the registration in the first place.
        if (newUser is null)
        {
            return Conflict(new { error = $"This phone number '{phone}' is already registered." });
        }

        // Business? newBusiness = null;

        // if (!string.IsNullOrWhiteSpace(businessName))
        // {
        //     // string name = businessName;
        //     string userId = newUser.id.ToString();
        //     string? registrationNo = cacNo;
        //     newBusiness = await _businessService.RegisterBusiness(
        //         businessName, userId, registrationNo, area
        //     );

        //     if (newBusiness is null)
        //     {
        //         return Conflict(new {
        //             error = $"User registered, but the business '{businessName}' was previously registered.",
        //             userId = newUser.id,
        //             business = businessName
        //         });
        //     }

        // }

        var response = new RegisterUserResponse
        {
            userCreated = true,
            userId = newUser.id,
        };

        return StatusCode(201, response);
    }
}