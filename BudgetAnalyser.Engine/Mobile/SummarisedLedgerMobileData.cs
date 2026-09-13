// ReSharper disable UnusedAutoPropertyAccessor.Global // Used by unit testing and debugging

namespace BudgetAnalyser.Engine.Mobile;

/// <summary>
///     A DTO type to export all data about the state of the ledger at a point in time.
/// </summary>
public class SummarisedLedgerMobileData
{
    /// <summary>
    ///     Instantiate a new instance of <see cref="SummarisedLedgerMobileData" />
    /// </summary>
    public SummarisedLedgerMobileData()
    {
        LedgerBuckets = new List<SummarisedLedgerBucket>();
    }

    /// <summary>
    ///     The date and time this object was exported from Budget Analyser
    /// </summary>
    public required DateTime Exported { get; set; }

    /// <summary>
    ///     The date and time transactions were last imported into BudgetAnalyser from the bank.
    /// </summary>
    public DateTime LastTransactionImport { get; set; }

    /// <summary>
    ///     All the ledger buckets in the ledger
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Global // Used by serialisation
    public List<SummarisedLedgerBucket> LedgerBuckets { get; private set; }

    /// <summary>
    ///     The date this month started
    /// </summary>
    public DateOnly StartOfMonth { get; set; }

    /// <summary>
    ///     The title of the budget
    /// </summary>
    public required string Title { get; set; }
}
