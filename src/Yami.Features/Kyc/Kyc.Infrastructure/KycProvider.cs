// Placeholder for the real KYC provider client.
// The actual endpoint is unknown yet, so this stub simulates a successful
// verification. Replace the body with a real HTTP call to the provider when
// the endpoint is known — the rest of the module won't need to change.
public class KycProvider : IKycProvider
{
    public async Task<ProviderVerificationResult> VerifyAsync(
        string entityType, string identityType, string identityNumber)
    {
        // TODO: replace with a real call to the KYC provider endpoint.
        await Task.CompletedTask;

        return new ProviderVerificationResult
        {
            status = VerificationStatus.Verified,
            message = "Placeholder: verification succeeded."
        };
    }
}
