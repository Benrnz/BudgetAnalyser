namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     A suggested (or user chosen) range of dates to import transactions for. Both dates are inclusive.
/// </summary>
/// <param name="Start">
///     The first date to import. This is null when there are no existing transactions to base a suggestion on, in which case the user must choose a start date.
/// </param>
/// <param name="End">The last date to import.</param>
public readonly record struct ImportDateRange(DateOnly? Start, DateOnly End)
{
    /// <summary>
    ///     Gets a value indicating whether there is nothing to import, because the start date is after the end date (the existing transactions are already up to date).
    /// </summary>
    public bool IsEmpty => Start is not null && Start.Value > End;
}
