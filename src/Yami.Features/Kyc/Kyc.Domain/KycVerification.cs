// The lifecycle of a KYC verification attempt.
// Kept in the Kyc module because the process of verifying is a KYC concern;
public enum VerificationStatus
{
    InProgress,   // submitted, provider still working (or awaiting manual review)
    Verified,     // provider confirmed the entity
    Failed,       // provider made a verdict: the entity was rejected
    Error         // technical failure (provider error, timeout, our own bug)
}

public class KycVerification
{
    public Guid id { get; set; }
    public Guid entityId { get; set; }        // the user or business being verified
    public string entityType { get; set; }    // "user" | "business"
    public string identityType { get; set; }  // "nin" | "bvn" | "cac"
    public string identityNumber { get; set; }
    public VerificationStatus status { get; set; } = VerificationStatus.InProgress;  // default to in progress for new verification records
    public string? failureReason { get; set; }    // set when status is Failed or Error
    public DateTime createdAt { get; set; }
    public DateTime? completedAt { get; set; }
}
