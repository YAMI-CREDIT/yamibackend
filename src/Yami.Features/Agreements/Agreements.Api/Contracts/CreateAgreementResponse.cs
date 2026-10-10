// Shape of the JSON body returned after creating a new agreement.
public class CreateAgreementResponse
{
    public bool agreementCreated { get; set; }
    public Guid agreementId { get; set; }
}
