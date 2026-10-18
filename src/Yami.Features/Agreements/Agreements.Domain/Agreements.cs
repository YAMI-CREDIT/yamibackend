public enum AgreementStatus
{
    Accepted,   // Accepted by the other party
    Rejected,     // rejected by the other party
    Pending,  // Waiting for the other party to respond
    InProgress,  // debtor is in the process of paying
    Settled,  // fully paid by debtor
}

public enum RepaymentPlan
{
    daily,    // debtor will pay daily,
    weekly,   // debtor will pay weekly,
    monthly,  // debtor will pay monthly,
    lumpSum, // debtor will pay one lump sum by the agreed date,
    custom,  // debtor will pay as per the agreed custom terms
}

public class Agreement
{
    public Guid id { get; set; }
    public Guid creditorId { get; set; }
    public string? creditorName { get; set; }
    public Guid debtorId { get; set; }
    public string? debtorName { get; set; }
    public bool creditorConsent { get; set; }
    public bool debtorConsent { get; set; }
    public string? purpose { get; set; }
    required public string title { get; set; }
    public decimal amountTotal { get; set; }
    public decimal? amountPaid { get; set; }
    required public string currency { get; set; }
    public DateOnly dueDate { get; set; }
    public DateTime createdAt { get; set; }
    public Guid createdBy { get; set; }
    public AgreementStatus  status { get; set; } = AgreementStatus.Pending;
    public DateTime? acceptedAt { get; set; }
    public DateTime? settledAt { get; set; }
    public RepaymentPlan repaymentPlan { get; set; } = RepaymentPlan.lumpSum;
    // public DateTime[] paymentdates { get; set; }
}