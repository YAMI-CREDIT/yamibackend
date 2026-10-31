public class Installment
{
    public Guid id { get; set; }
    public Guid agreementId { get; set; }
    public DateOnly dueDate { get; set; }
    public decimal amountDue { get; set; }
    public DateTime? acceptedAt { get; set; }
}