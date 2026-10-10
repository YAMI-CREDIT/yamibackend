// Shape of the JSON body the frontend sends to create a new agreement.
// The creditorId must match the authenticated user; the backend verifies it
// against the Cognito sub claim before proceeding.
public class CreateAgreementRequest
{
    public Guid creditorId { get; set; }
    public Guid debtorId { get; set; }
    required public string title { get; set; }
    public decimal amountTotal { get; set; }
    public DateOnly dueDate { get; set; }
}
