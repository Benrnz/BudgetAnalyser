using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A state persistence Dto for Transactions.
/// </summary>
/// <seealso cref="IPersistentApplicationStateObject" />
public class StatementApplicationState : IPersistentApplicationStateObject
{
    /// <summary>
    ///     DEPRECATED - no longer supported. Kept here for persistence compatibility.
    ///     Gets or sets a value indicating if the user prefers to sort and group by bucket rather than by date.
    /// </summary>
    public bool? SortByBucket { get; init; }

    /// <summary>
    ///     Gets the order in which this object should be loaded.
    /// </summary>
    public int LoadSequence => 20;
}
