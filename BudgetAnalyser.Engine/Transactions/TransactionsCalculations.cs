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

        return startDate.DurationInWeeks(endDate) / 2;
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
}
