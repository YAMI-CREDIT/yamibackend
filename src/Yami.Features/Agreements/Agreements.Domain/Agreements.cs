public class Agreement
{
    public Guid id { get; set; }
    public Guid creditorId { get; set; }
    public string creditorName { get; set; }
    public Guid debtorId { get; set; }
    public string debtorName { get; set; }
    public string title { get; set; }
    public decimal amountTotal { get; set; }
    public decimal? amountPaid { get; set; }
    public DateTime dueDate { get; set; }
    public DateTime createdAt { get; set; }
}