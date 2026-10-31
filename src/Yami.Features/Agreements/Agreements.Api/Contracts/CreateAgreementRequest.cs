// Shape of the JSON body the frontend sends to create a new agreement.
// The backend verifies the party IDs against the authenticated user's claim.
public class CreateAgreementRequest
{
    public Guid creditorId { get; set; }
    public Guid debtorId { get; set; }
    public string? purpose { get; set; }
    required public string title { get; set; }
    public decimal amountTotal { get; set; }
    public DateOnly dueDate { get; set; }
    public RepaymentPlan repaymentPlan { get; set; } = RepaymentPlan.lumpSum;
}
