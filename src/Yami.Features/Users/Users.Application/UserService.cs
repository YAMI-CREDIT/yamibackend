using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UsersDbContext _db;

    public UserService(UsersDbContext db)
    {
        _db = db;
    }

    // This is the actual Registration logic
    public async Task<User?> RegisterUser(
        string phone, string name, string userSubId, string? dateOfBirth,
        string? email, bool termsAccepted=true, string? area=null,
        string? identityType=null, string? identityNumber=null,
        string? userType=null)
    {
        bool alreadyExists = await _db.Users
            .AnyAsync(u => u.phone == phone);

        if (alreadyExists)
        {
            return null;   
        }

        var newUser = new User
        {
            phone = phone,
            name = name,
            userSubId = userSubId,
            email = email,
            termsAccepted=termsAccepted,
            area = area,
            identityType = identityType,
            identityNumber = identityNumber,
            userType = userType,
            dateOfBirth = dateOfBirth,
            createdAt = DateTime.UtcNow,
            verified = false
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return newUser;
    }

    // This logic is an example to get user details from dB.
    // I'll leave the full implementation till later
    public async Task<User?> GetUser(Guid id)
    {
        Console.WriteLine($"Getting user by ID: {id}");
        return await _db.Users.FindAsync(id);
    }
}