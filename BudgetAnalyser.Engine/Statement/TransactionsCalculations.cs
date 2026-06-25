using System.Diagnostics;

namespace BudgetAnalyser.Engine.Statement;

public static class TransactionsCalculations
{
    /// <summary>
    ///     Calculates the duration in months from the beginning of the period to the end.
    /// </summary>
    /// <param name="criteria">
    ///     The criteria that is currently applied to the Statement. Pass in null to use first and last
    ///     statement dates.
    /// </param>
    /// <param name="transactions">The list of transactions to use to determine duration.</param>
    public static int CalculateDurationInFortnights(GlobalFilterCriteria? criteria, IEnumerable<Transaction> transactions)
    {
        var tuple = ValidateCriteria(criteria, transactions);
        return tuple.Item1.DurationInWeeks(tuple.Item2) / 2;
    }

    /// <summary>
    ///     Calculates the duration in months from the beginning of the period to the end.
    /// </summary>
    /// <param name="criteria">
    ///     The criteria that is currently applied to the Statement. Pass in null to use first and last
    ///     statement dates.
    /// </param>
    /// <param name="transactions">The list of transactions to use to determine duration.</param>
    public static int CalculateDurationInMonths(GlobalFilterCriteria? criteria, IEnumerable<Transaction> transactions)
    {
        var tuple = ValidateCriteria(criteria, transactions);
        return tuple.Item1.DurationInMonths(tuple.Item2);
    }

    private static (DateOnly, DateOnly) ValidateCriteria(GlobalFilterCriteria? criteria, IEnumerable<Transaction> transactions)
    {
        var list = transactions.ToList();
        var minDate = DateOnly.MaxValue;
        var maxDate = DateOnly.MinValue;

        if (criteria is not null && !criteria.Cleared)
        {
            if (criteria.BeginDate is not null)
            {
                minDate = criteria.BeginDate.Value;
                Debug.Assert(criteria.EndDate is not null);
                maxDate = criteria.EndDate.Value;
            }
        }
        else
        {
            minDate = list.Min(t => t.Date);
            maxDate = list.Max(t => t.Date);
        }

        return (minDate, maxDate);
    }
}
