namespace BudgetAnalyser.Engine.Transactions;

public static class TransactionsCalculations
{
    /// <summary>
    ///     Calculates the duration in months from the beginning of the period to the end.
    /// </summary>
    /// <param name="transactions">The list of transactions to use to determine duration.</param>
    /// <param name="startDate">The start date for the calculation.</param>
    /// <param name="endDate">The end date for the calculation.</param>
    public static int CalculateDurationInFortnights(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
        {
            return 0;
        }

        return Math.Max(1, startDate.DurationInWeeks(endDate) / 2);

    }

    /// <summary>
    ///     Calculates the duration in months from the beginning of the period to the end.
    /// </summary>
    /// <param name="criteria">
    ///     The criteria that is currently applied to the transactions list model. Pass in null to use first and last transaction dates.
    /// </param>
    /// <param name="transactions">The list of transactions to use to determine duration.</param>
    public static int CalculateDurationInMonths(GlobalFilterCriteria? criteria, IEnumerable<Transaction> transactions)
    {
        if (criteria is null || criteria.Cleared || criteria.BeginDate is null || criteria.EndDate is null)
        {
            var myCopy = transactions.ToList();
            var date1 = myCopy.Min(t => t.Date);
            var date2 = myCopy.Max(t => t.Date);
            return date1.DurationInMonths(date2);
        }

        return criteria.BeginDate.Value.DurationInMonths(criteria.EndDate.Value);
    }

    public static int CalculateDurationInMonths(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
        {
            return 0;
        }

        return startDate.DurationInMonths(endDate);
    }

    /// <summary>
    ///     Calculates the suggested range of dates to import new transactions for.
    ///     The start is the day after the latest existing transaction, or null if there are no existing transactions.
    ///     The end is the most recent business day before <paramref name="today" />. A business day is Monday to Friday and not a New Zealand public holiday. Today is never included
    ///     because today's transactions are not final, and weekend and holiday transactions are not final until the next business day has been processed by the bank.
    /// </summary>
    /// <param name="existingTransactions">All the transactions currently loaded.</param>
    /// <param name="today">The current date. Callers should pass <see cref="DateOnlyExt.Today" />.</param>
    public static ImportDateRange CalculateSuggestedImportDateRange(IEnumerable<Transaction> existingTransactions, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(existingTransactions);

        DateOnly? start = null;
        foreach (var transaction in existingTransactions)
        {
            var nextDay = transaction.Date.AddDays(1);
            if (start is null || nextDay > start.Value)
            {
                start = nextDay;
            }
        }

        return new ImportDateRange(start, PreviousBusinessDay(today));
    }

    /// <summary>
    ///     Finds the most recent business day strictly before <paramref name="today" />. A business day is Monday to Friday and not a New Zealand public holiday.
    /// </summary>
    internal static DateOnly PreviousBusinessDay(DateOnly today)
    {
        // Holidays are always calculated for whole calendar years, the holiday calculator only returns one date per holiday per call, and short ranges cause errors to be logged.
        // The previous year is included so that walking back from early January into December works.
        var holidays = new HashSet<DateOnly>();
        for (var year = today.Year - 1; year <= today.Year; year++)
        {
            holidays.UnionWith(NewZealandPublicHolidays.CalculateHolidays(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31)));
        }

        var date = today.AddDays(-1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || holidays.Contains(date))
        {
            date = date.AddDays(-1);
        }

        return date;
    }
}
