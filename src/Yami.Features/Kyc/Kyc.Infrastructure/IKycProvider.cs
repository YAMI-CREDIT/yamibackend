// The external KYC verification provider (the real endpoint is unknown yet).
// This is the seam where the actual provider client will plug in.
public interface IKycProvider
{
    Task<ProviderVerificationResult> VerifyAsync(
        string entityType, string identityType, string identityNumber);
}

// What the provider tells us. The real provider's response shape will map onto
// this. `status` is InProgress when the provider needs more details / is still
// working, Verified when it confirms the entity, Failed when it rejects it.
public class ProviderVerificationResult
{
    public VerificationStatus status { get; set; }
    public string? message { get; set; }
}
