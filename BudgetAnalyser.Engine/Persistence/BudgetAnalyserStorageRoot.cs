using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     A Dto object to store the top level Budget Analyser database file.
/// </summary>
public class BudgetAnalyserStorageRoot
{
    /// <summary>
    ///     Gets or sets the budget collection root dto.
    /// </summary>
    public required StorageBranch BudgetCollectionRootDto { get; init; }

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
    public required StorageBranch LedgerBookRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the ledger reconciliation to-do task collection.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
    public List<ToDoTaskDto> LedgerReconciliationToDoCollection { get; init; } = new();

    /// <summary>
    ///     Gets or sets the matching rules collection root dto.
    /// </summary>
    public required StorageBranch MatchingRulesCollectionRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the statement model root dto.
    /// </summary>
    public required StorageBranch StatementModelRootDto { get; init; }

    /// <summary>
    ///     Gets or sets the Widget Collection root dto.
    /// </summary>
    public required StorageBranch WidgetCollectionRootDto { get; init; }
}
