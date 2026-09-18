public interface IBusinessService
{
    Task<Business?> RegisterBusiness(
        string name, string userId,
        string? registrationNo, string area);
    Task<Business?> GetBusiness(string id);
}