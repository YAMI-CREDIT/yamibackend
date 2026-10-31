// Shape of the JSON body returned after creating a new agreement.
public class CreateAgreementResponse
{
    public bool agreementCreated { get; set; }
    public Guid agreementId { get; set; }
    public string currency { get; set; } = string.Empty;
    public IReadOnlyList<CreateAgreementInstallmentResponse> installments { get; set; } = [];
}

public class CreateAgreementInstallmentResponse
{
    public DateOnly dueDate { get; set; }
    public decimal amountDue { get; set; }
}
