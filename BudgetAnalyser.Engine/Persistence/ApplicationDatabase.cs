using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     A top level class that contains reference information to all other data stores in the application.
///     This class is not intended to contain full domain objects rather meta-data that points to the objects.
///     Prefer to use just the domain objects required, rather than over using an object that contains references to
///     everything.
/// </summary>
public class ApplicationDatabase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationDatabase" /> class.
    /// </summary>
    public ApplicationDatabase()
    {
        LedgerReconciliationToDoCollection = new ToDoCollection();
    }

    /// <summary>
    ///     Gets the budget collection storage key.
    ///     This is used to locate and load the <see cref="BudgetCollection" />.
    /// </summary>
    public string BudgetCollectionStorageKey { get; internal set; } = string.Empty;

    /// <summary>
    ///     Gets the name of the main Budget Analyser Data file.
    /// </summary>
    public string? FileName { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether the Application database is encrypted when stored on disk.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is encrypted; otherwise, <c>false</c>.
    /// </value>
    public bool IsEncrypted { get; internal set; }

    /// <summary>
    ///     Gets the ledger book storage key.
    ///     This is used to locate and load the <see cref="LedgerBook" />.
    /// </summary>
    public string LedgerBookStorageKey { get; internal set; } = string.Empty;

    /// <summary>
    ///     Gets the ledger reconciliation to do collection.
    ///     This contains persistent tasks that the user has created.
    ///     System generated tasks are not persisted.
    /// </summary>
    public ToDoCollection LedgerReconciliationToDoCollection { get; internal set; }

    /// <summary>
    ///     Gets the matching rules collection storage key.
    ///     This is used to locate and load a list of <see cref="MatchingRule" />s.
    /// </summary>
    public string MatchingRulesCollectionStorageKey { get; internal set; } = string.Empty;

    /// <summary>
    ///     Gets the statement model storage key.
    ///     This is used to locate and load the <see cref="StatementModel" />.
    /// </summary>
    public string StatementModelStorageKey { get; internal set; } = string.Empty;

    /// <summary>
    ///     Gets the storage path that identifies this budget analyser file.
    /// </summary>
    private string StoragePath => Path.GetDirectoryName(FileName) ?? ".\\";

    /// <summary>
    ///     Closes this instance.
    /// </summary>
    public void Close()
    {
        BudgetCollectionStorageKey = string.Empty;
        FileName = null;
        LedgerBookStorageKey = string.Empty;
        MatchingRulesCollectionStorageKey = string.Empty;
        MatchingRulesCollectionStorageKey = string.Empty;
        StatementModelStorageKey = string.Empty;
        LedgerReconciliationToDoCollection = new ToDoCollection();
    }

    /// <summary>
    ///     Builds and returns the full filename and path.
    /// </summary>
    public virtual string FullPath(string fileName)
    {
        return Path.Combine(StoragePath, fileName);
    }
}
