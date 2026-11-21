public class Installment
{
    public Guid id { get; set; }
    public Guid agreementId { get; set; }
    public DateOnly dueDate { get; set; }
    public decimal amountDue { get; set; }
    public DateTime? acceptedAt { get; set; }
    public bool debtorPaid { get; set; }
    public DateTime? debtorPaidAt { get; set; }
    public bool creditorConfirmedPayment { get; set; }
    public DateTime? creditorConfirmedPaymentAt { get; set; }
}