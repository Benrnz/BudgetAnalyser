namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     A task item for use with the <see cref="ToDoCollection" />.
/// </summary>
public class ToDoTask
{
    /// <summary>
    ///     Gets a value indicating whether this task can be deleted by the user.
    /// </summary>
    public bool CanDelete { get; init; } = true;

    /// <summary>
    ///     Gets the description of the task.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the task is system generated.
    /// </summary>
    public bool SystemGenerated { get; init; }
}
