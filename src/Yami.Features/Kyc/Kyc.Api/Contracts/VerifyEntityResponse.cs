// Shape of the JSON body returned after a KYC verification attempt.
// The POST always returns InProgress (202); the final outcome is read via the status endpoint.
public class VerifyEntityResponse
{
    public string verificationStatus { get; set; }  // always "InProgress" for this endpoint
    public string? message { get; set; }
}
