// Shape of the JSON body the frontend sends to to backend for registering a new user.
// I'm purposely skipping Id at this stage and using Auto generated IDs
// The plan is to extract the sub from access token granted by cognito and use it as the id
public class RegisterUserRequest
{   
    public string phone { get; set; }
    public string name { get; set; }
    public string userType { get; set; }
    public string dateOfBirth { get; set; }
}