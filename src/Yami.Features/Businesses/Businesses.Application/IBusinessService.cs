public interface IBusinessService
{
    Task<Business?> RegisterBusiness(
        string name, Guid userId,
        string? registrationNo, string area);
    Task<Business?> GetBusiness(Guid id);
    Task<bool> SetVerified(Guid id, bool verified);
}