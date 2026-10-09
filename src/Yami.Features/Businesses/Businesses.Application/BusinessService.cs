using Microsoft.EntityFrameworkCore;
public class BusinessService : IBusinessService
{
    private readonly BusinessesDbContext _db;

    public  BusinessService(BusinessesDbContext db)
    {
        _db = db;
    }

    // This is the actual Registration logic
    public async Task<Business?> RegisterBusiness(
        string name, string userId,
        string? registrationNo, string area)
    {
        bool alreadyExists = await _db.Businesses
            .AnyAsync(b => b.name == name);

        if (alreadyExists)
        {
            return null;   
        }

        var newBusiness = new Business
        {
            name = name,
            userId = userId,
            registrationNo = registrationNo, 
            area = area,
            createdAt = DateTime.UtcNow
        };

        _db.Businesses.Add(newBusiness);
        await _db.SaveChangesAsync();

        return newBusiness;
    }

    // Placeholder logic to get business details from DB, to be implemented later if required
    public async Task<Business?> GetBusiness(string id)
    {
        Console.WriteLine($"Getting business by ID: {id}");
        return await _db.Businesses.FindAsync(id);
    }

    // Flips the business's final verification outcome. Returns false if the
    // business doesn't exist. The Kyc module calls this with the provider's result.
    public async Task<bool> SetVerified(Guid id, bool verified)
    {
        var business = await _db.Businesses.FindAsync(id);
        if (business is null)
        {
            return false;
        }

        business.verified = verified;
        await _db.SaveChangesAsync();
        return true;
    }
}