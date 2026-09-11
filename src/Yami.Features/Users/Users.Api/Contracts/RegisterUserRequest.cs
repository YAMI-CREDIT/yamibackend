// Shape of the JSON body the frontend sends to to backend for registering a new user.
// I'm purposely skipping Id at this stage and using Auto generated IDs
// Each user gets a GUID on insertion into the database
public class RegisterUserRequest
{   
    public string phone { get; set; }
    public string name { get; set; }
    public string? email { get; set; }
    public string? businessName { get; set; }
    public string? area { get; set; }
    public string? identityType { get; set; }
    public string? identityNumber  { get; set; }
    public string? userType { get; set; }
    public string? dateOfBirth { get; set; }
}