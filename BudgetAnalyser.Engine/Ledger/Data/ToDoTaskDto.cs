namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto object to persist a ToDoTask
/// </summary>
public record ToDoTaskDto(bool CanDelete, string Description, bool SystemGenerated);
