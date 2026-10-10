// Shape of the JSON body the frontend sends to to backend for registering a new user.
// Each user gets a GUID on insertion into the database
public class RegisterUserRequest
{   
    required public string phone { get; set; }
    required public string name { get; set; }
    public string? email { get; set; }
    public string? dateOfBirth { get; set; }
}