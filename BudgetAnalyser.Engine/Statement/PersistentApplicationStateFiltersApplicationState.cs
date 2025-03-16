using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A state persistence Dto for global filters
/// </summary>
/// <seealso cref="IPersistentApplicationStateObject" />
public class PersistentFiltersApplicationState : IPersistentApplicationStateObject
{
    /// <summary>
    ///     Gets or sets the date to begin filtering from.
    /// </summary>
    public DateOnly? BeginDate { get; init; }

    /// <summary>
    ///     Gets or sets the end date.
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    ///     Gets the order in which this object should be loaded.
    /// </summary>
    public int LoadSequence => 50;
}
