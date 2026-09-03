// This interface is a CONTRACT. It says "any UserService must be able
// to do this," without saying HOW. This is what lets us swap
// implementations later (e.g. a fake one for tests) without touching
// the controller at all.
public interface IUserService
{
    // These methods are implemented in UserService.cs
    Task<User?> RegisterUser(string phone, string name, string userType, string dateOfBirth);
    Task<User?> GetUser(string id);
}