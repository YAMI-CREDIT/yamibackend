// The authenticated user's installments due within the requested window,
// across accepted agreements where they owe or expect payment.
public class GetUpcomingInstallmentsResponse
{
    public int days { get; set; }
    public string direction { get; set; } = string.Empty;
    public IReadOnlyList<UpcomingInstallmentResponse> installments { get; set; } = [];
}

public class UpcomingInstallmentResponse
{
    public Guid agreementId { get; set; }
    public string title { get; set; } = string.Empty;
    public string currency { get; set; } = string.Empty;
    public DateOnly dueDate { get; set; }
    public decimal amountDue { get; set; }
}
