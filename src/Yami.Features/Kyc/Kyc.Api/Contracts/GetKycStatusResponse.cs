// Shape of the JSON body returned when polling a KYC verification status.
public class GetKycStatusResponse
{
    public string verificationStatus { get; set; }  // "NotStarted" | "InProgress" | "Verified" | "Failed" | "Error"
    public string? message { get; set; }
    public string? failureReason { get; set; }     // set only when status is "Failed"
    public DateTime? completedAt { get; set; }
}
