// Shape of the JSON body for creating an agreement with an unregistered debtor party.
public class CreateAgreementInviteDebtorRequest
{
    public required string debtorPhone { get; set; }
    public string? purpose { get; set; }
    required public string title { get; set; }
    public decimal amountTotal { get; set; }
    public DateOnly dueDate { get; set; }
    public RepaymentPlan repaymentPlan { get; set; } = RepaymentPlan.lumpSum;
}
