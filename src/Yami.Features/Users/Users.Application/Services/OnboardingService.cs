using Microsoft.EntityFrameworkCore;

public class OnboardingService : IOnboardingService
{
    private readonly UsersDbContext _db;

    public OnboardingService(UsersDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessProfile?> GetAsync(Guid userId)
    {
        return await _db.BusinessProfiles.SingleOrDefaultAsync(bp => bp.UserId == userId);
    }

    public async Task<BusinessProfile> UpsertAsync(Guid userId, BusinessProfileInput input)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new AuthException("User not found.", 404);
        }

        var profile = await _db.BusinessProfiles.SingleOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null)
        {
            profile = new BusinessProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.BusinessProfiles.Add(profile);
        }
        else
        {
            profile.UpdatedAt = DateTime.UtcNow;
        }

        profile.BusinessName = input.BusinessName;
        profile.BusinessType = input.BusinessType;
        profile.Industry = input.Industry;
        profile.RegistrationNumber = input.RegistrationNumber;
        profile.BusinessAddress = input.BusinessAddress;
        profile.BusinessEmail = input.BusinessEmail;
        profile.BusinessPhone = input.BusinessPhone;
        profile.YearsInOperation = input.YearsInOperation;
        profile.EstimatedMonthlyRevenue = input.EstimatedMonthlyRevenue;
        profile.IsCompleted = true;

        await _db.SaveChangesAsync();

        return profile;
    }
}
