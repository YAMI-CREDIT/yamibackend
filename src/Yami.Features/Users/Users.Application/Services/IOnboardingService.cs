public interface IOnboardingService
{
    Task<BusinessProfile?> GetAsync(Guid userId);
    Task<BusinessProfile> UpsertAsync(Guid userId, BusinessProfileInput input);
}

// The fields collected on the onboarding screen once a user has signed up.
public record BusinessProfileInput(
    string BusinessName,
    string? BusinessType,
    string? Industry,
    string? RegistrationNumber,
    string? BusinessAddress,
    string? BusinessEmail,
    string? BusinessPhone,
    string? YearsInOperation,
    string? EstimatedMonthlyRevenue);
