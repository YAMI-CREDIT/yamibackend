using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UsersDbContext _db;

    public UserService(UsersDbContext db)
    {
        _db = db;
    }

    // This is the actual Registration logic
    public async Task<User?> RegisterUser(string phone, string name, string userType, string dateOfBirth)
    {
        bool alreadyExists = await _db.Users
            .AnyAsync(u => u.phone == phone);

        if (alreadyExists)
        {
            return null;   
        }

        var newUser = new User
        {
            Phone = phone,
            Name = name
            UserType = userType
            DateOfBirth = dateOfBirth
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return newUser;
    }

    // This logic is an example to get user details from dB.
    // I'll leave the implementation till later
    public async Task<User?> GetUser(string id)
    {
        console.WriteLine($"Getting user by ID: {id}");
        // return await _db.Users.FindAsync(id);
    }
}