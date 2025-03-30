using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     A Dto object to store the top level Budget Analyser database file.
/// </summary>
public class BudgetAnalyserStorageRoot2
{
    /// <summary>
    ///     Gets or sets the budget collection root dto.
    /// </summary>
    public required string BudgetCollectionRootDto { get; init; }

    /// <summary>
    ///     The filter that is applied to transactions, reports, and is used to determine period for month end reconciliation.
    /// </summary>
    public required GlobalFilterDto Filter { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether the data files are encrypted.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is encrypted; otherwise, <c>false</c>.
    /// </value>
    public bool IsEncrypted { get; init; }

    /// <summary>
    ///     Gets or sets the ledger book root dto.
    /// </summary>
    public required string LedgerBookRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the ledger reconciliation to-do task collection.
    /// </summary>
    public List<ToDoTaskDto> LedgerReconciliationToDoCollection { get; init; } = new();

    /// <summary>
    ///     Gets or sets the matching rules collection root dto.
    /// </summary>
    public required string MatchingRulesCollectionRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the statement model root dto.
    /// </summary>
    public required string StatementModelRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the Widget Collection root dto.
    /// </summary>
    public required string WidgetCollectionRootDto { get; init; }
}
