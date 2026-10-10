public enum AgreementStatus
{
    Accepted,   // Accepted by the other party
    Rejected,     // rejected by the other party
    Pending,  // Waiting for the other party to respond
    InProgress,  // debtor is in the process of paying
    Settled,  // fully paid by debtor
}

public class Agreement
{
    public Guid id { get; set; }
    public Guid creditorId { get; set; }
    public string creditorName { get; set; }
    public Guid debtorId { get; set; }
    public string debtorName { get; set; }
    public bool creditorConsent { get; set; }
    public bool debtorConsent { get; set; }
    public string title { get; set; }
    public decimal amountTotal { get; set; }
    public decimal? amountPaid { get; set; }
    public string currency { get; set; }
    public DateOnly dueDate { get; set; }
    public DateTime createdAt { get; set; }
    public string createdBy { get; set; }
    public AgreementStatus  status { get; set; } = AgreementStatus.Pending;
    public DateTime? acceptedAt { get; set; }
    public DateTime? settledAt { get; set; }
    // public string paymentTerms { get; set; }
    // public DateTime[] paymentdates { get; set; }
}