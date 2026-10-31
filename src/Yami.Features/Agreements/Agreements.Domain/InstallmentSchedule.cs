public static class InstallmentSchedule
{
    public static List<Installment>? Create(Agreement agreement)
    {
        if (agreement.amountTotal != decimal.Round(agreement.amountTotal, 2))
        {
            return null;
        }

        var dueDates = GetDueDates(
            agreement.repaymentPlan,
            DateOnly.FromDateTime(agreement.createdAt),
            agreement.dueDate);

        if (dueDates is null)
        {
            return null;
        }

        var amountPerInstallment = decimal.Floor(
            agreement.amountTotal / dueDates.Count * 100m) / 100m;

        if (amountPerInstallment <= 0)
        {
            return null;
        }

        return dueDates.Select((dueDate, index) => new Installment
        {
            agreementId = agreement.id,
            dueDate = dueDate,
            amountDue = index == dueDates.Count - 1
                ? agreement.amountTotal - amountPerInstallment * index
                : amountPerInstallment
        }).ToList();
    }

    private static List<DateOnly>? GetDueDates(
        RepaymentPlan repaymentPlan,
        DateOnly scheduleStartDate,
        DateOnly finalDueDate)
    {
        if (finalDueDate <= scheduleStartDate)
        {
            return null;
        }

        if (repaymentPlan is RepaymentPlan.lumpSum or RepaymentPlan.custom)
        {
            return [finalDueDate];
        }

        var dueDates = new List<DateOnly>();
        for (var interval = 1; ; interval++)
        {
            var nextDueDate = repaymentPlan switch
            {
                RepaymentPlan.daily => scheduleStartDate.AddDays(interval),
                RepaymentPlan.weekly => scheduleStartDate.AddDays(interval * 7),
                RepaymentPlan.monthly => scheduleStartDate.AddMonths(interval),
                _ => (DateOnly?)null
            };

            if (nextDueDate is null)
            {
                return null;
            }

            if (nextDueDate > finalDueDate)
            {
                break;
            }

            dueDates.Add(nextDueDate.Value);
            if (nextDueDate == finalDueDate)
            {
                return dueDates;
            }
        }

        if (dueDates.Count == 0)
        {
            return null;
        }

        if (dueDates[^1] != finalDueDate)
        {
            dueDates.Add(finalDueDate);
        }

        return dueDates;
    }
}