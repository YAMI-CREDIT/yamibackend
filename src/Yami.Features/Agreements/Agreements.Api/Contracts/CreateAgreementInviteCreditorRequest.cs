// Shape of the JSON body for creating an agreement with an unregistered creditor party.
public class CreateAgreementInviteCreditorRequest
{
    public required string creditorPhone { get; set; }
    required public string title { get; set; }
    public decimal amountTotal { get; set; }
    public DateOnly dueDate { get; set; }
    public RepaymentPlan repaymentPlan { get; set; } = RepaymentPlan.lumpSum;
}
