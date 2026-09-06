using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UsersDbContext _db;

    public UserService(UsersDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users
            .Include(u => u.BusinessProfile)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
