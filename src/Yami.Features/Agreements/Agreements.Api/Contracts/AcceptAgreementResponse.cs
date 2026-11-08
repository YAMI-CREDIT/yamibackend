public class AcceptAgreementResponse
{
    public bool agreementAccepted { get; set; }
    public Guid agreementId { get; set; }
    public IReadOnlyList<AgreementInstallmentResponse> installments { get; set; } = [];
}