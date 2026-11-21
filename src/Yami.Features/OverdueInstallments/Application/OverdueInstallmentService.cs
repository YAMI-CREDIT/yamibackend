using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class OverdueInstallmentService : IOverdueInstallmentService
{
    private readonly AgreementsDbContext _agreementsDb;
    private readonly UsersDbContext _usersDb;
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OverdueInstallmentService> _logger;

    public OverdueInstallmentService(
        AgreementsDbContext agreementsDb,
        UsersDbContext usersDb,
        ISmsSender smsSender,
        IEmailSender emailSender,
        ILogger<OverdueInstallmentService> logger)
    {
        _agreementsDb = agreementsDb;
        _usersDb = usersDb;
        _smsSender = smsSender;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<int> ProcessOverdueInstallments(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // An installment is overdue when its due date has passed, it has not
        // been paid by the debtor, and the parent agreement is still active.
        var overdue = await (
            from installment in _agreementsDb.Installments.AsNoTracking()
            join agreement in _agreementsDb.Agreements.AsNoTracking()
                on installment.agreementId equals agreement.id
            where agreement.status == AgreementStatus.InProgress
                && installment.dueDate < today
                && !installment.debtorPaid
            select new { installment, agreement })
            .OrderBy(item => item.installment.dueDate)
            .ToListAsync(cancellationToken);

        if (overdue.Count == 0)
        {
            _logger.LogInformation(
                "No overdue installments found as of {Today}.",
                today.ToString("yyyy-MM-dd"));
            return 0;
        }

        var totalOutstanding = overdue
            .Sum(item => item.installment.amountDue);

        _logger.LogInformation(
            "Found {Count} overdue installment(s) totalling {Total} across {AgreementCount} agreement(s) as of {Today}.",
            overdue.Count,
            totalOutstanding.ToString("0.00"),
            overdue.Select(item => item.agreement.id).Distinct().Count(),
            today.ToString("yyyy-MM-dd"));

        foreach (var item in overdue)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var daysOverdue = (today.ToDateTime(TimeOnly.MinValue)
                - item.installment.dueDate.ToDateTime(TimeOnly.MinValue)).Days;

            _logger.LogInformation(
                "Overdue: agreement {AgreementId} ({Title}), debtor {DebtorId}, due {DueDate}, {Amount} {Currency}, {DaysOverdue} day(s) overdue.",
                item.agreement.id,
                item.agreement.title,
                item.agreement.debtorId,
                item.installment.dueDate.ToString("yyyy-MM-dd"),
                item.installment.amountDue.ToString("0.00"),
                item.agreement.currency,
                daysOverdue);

            await NotifyDebtor(item.agreement, item.installment, daysOverdue, cancellationToken);
        }

        _logger.LogInformation(
            "Overdue installments job complete: {Count} installment(s) processed.",
            overdue.Count);

        return overdue.Count;
    }

    private async Task NotifyDebtor(
        Agreement agreement,
        Installment installment,
        int daysOverdue,
        CancellationToken cancellationToken)
    {
        // Resolve the debtor's current contact details from the Users table.
        var debtor = await _usersDb.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.id == agreement.debtorId,
                cancellationToken);

        if (debtor is null)
        {
            _logger.LogWarning(
                "No user record found for debtor {DebtorId} on agreement {AgreementId}; skipping notification.",
                agreement.debtorId,
                agreement.id);
            return;
        }

        var firstName = debtor.name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? debtor.name;
        var dueDate = installment.dueDate.ToString("yyyy-MM-dd");
        var amount = installment.amountDue.ToString("0.00");

        // SMS reminder (non-fatal on failure).
        if (!string.IsNullOrWhiteSpace(debtor.phone))
        {
            var smsMessage =
                $"Hi {firstName},\n" +
                $"You have an overdue installment of {amount} {agreement.currency} " +
                $"on agreement \"{agreement.title}\" that was due {dueDate}. " +
                $"Please log in to Yami to settle it.\n" +
                "Thank you.";

            try
            {
                var result = await _smsSender.SendSms(debtor.phone, smsMessage);
                _logger.LogInformation(
                    "Sent overdue-installment SMS to user {UserId} for agreement {AgreementId} (code {Code}).",
                    debtor.id,
                    agreement.id,
                    result.Code);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to send overdue-installment SMS to user {UserId} for agreement {AgreementId}.",
                    debtor.id,
                    agreement.id);
            }
        }

        // Email reminder (non-fatal on failure).
        if (!string.IsNullOrWhiteSpace(debtor.email))
        {
            var encodedName = System.Net.WebUtility.HtmlEncode(debtor.name);
            var encodedTitle = System.Net.WebUtility.HtmlEncode(agreement.title);
            var encodedCurrency = System.Net.WebUtility.HtmlEncode(agreement.currency);

            var htmlContent =
                $"Hi {encodedName},<br>" +
                "You have an overdue installment on YAMI.<br>" +
                $"Agreement: {encodedTitle}<br>" +
                $"Amount due: {amount} {encodedCurrency}<br>" +
                $"Due date: {dueDate}<br>" +
                $"Days overdue: {daysOverdue}<br>" +
                "Please log in to Yami to settle this installment.";

            try
            {
                await _emailSender.SendEmail(
                    debtor.email,
                    "You have an overdue installment on YAMI",
                    htmlContent);
                _logger.LogInformation(
                    "Sent overdue-installment email to user {UserId} for agreement {AgreementId}.",
                    debtor.id,
                    agreement.id);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to send overdue-installment email to user {UserId} for agreement {AgreementId}.",
                    debtor.id,
                    agreement.id);
            }
        }
    }
}
