public interface IOverdueInstallmentService
{
    // Finds every unpaid installment on an in-progress agreement whose due
    // date has passed, logs a summary, and sends the debtor a reminder.
    // Returns the number of overdue installments processed.
    Task<int> ProcessOverdueInstallments(CancellationToken cancellationToken = default);
}
