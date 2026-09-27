// The Kyc module's public contract. The controller depends on this, never on
// the provider or the DbContext directly.
public interface IKycService
{
    Task<KycVerificationResult> VerifyEntity(
        Guid entityId, string entityType, string identityType, string identityNumber);
    Task<KycVerification?> GetStatus(Guid entityId, string entityType);
}

// The outcome of a verification attempt, ready for the controller to map to a
// status code.
public class KycVerificationResult
{
    public VerificationStatus status { get; set; }
    public string? message { get; set; }
}
