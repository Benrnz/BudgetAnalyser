using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto object to persist a ToDoTask
/// </summary>
public class ToDoTaskDto
{
    /// <summary>
    ///     Gets or sets a value indicating whether this instance can be deleted by the user.
    /// </summary>
    public bool CanDelete { [UsedImplicitly] get; set; }

    /// <summary>
    ///     Gets or sets the description of the task.
    /// </summary>
    public string Description { [UsedImplicitly] get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether task is system generated.
    /// </summary>
    public bool SystemGenerated { [UsedImplicitly] get; set; }
}
