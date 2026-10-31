public class GetAgreementInstallmentsResponse
{
    public Guid agreementId { get; set; }
    public IReadOnlyList<AgreementInstallmentResponse> installments { get; set; } = [];
}

public class AgreementInstallmentResponse
{
    public DateOnly dueDate { get; set; }
    public decimal amountDue { get; set; }
}