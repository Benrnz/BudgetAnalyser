namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto object to persist a ToDoTask
/// </summary>
public class ToDoTaskDto
{
    /// <summary>
    ///     Gets or sets a value indicating whether this instance can be deleted by the user.
    /// </summary>
    public bool CanDelete { get; init; }

    /// <summary>
    ///     Gets or sets the description of the task.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether task is system generated.
    /// </summary>
    public bool SystemGenerated { get; init; }
}
