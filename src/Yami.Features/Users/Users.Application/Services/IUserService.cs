// This interface is a CONTRACT for read-only user profile lookups.
// Signup and authentication live in IAuthService; onboarding/business details in IOnboardingService.
public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
}
