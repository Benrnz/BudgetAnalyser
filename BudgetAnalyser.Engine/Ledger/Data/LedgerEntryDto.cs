namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerEntry" />.
/// </summary>
public class LedgerEntryDto
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryDto" /> class.
    /// </summary>
    public LedgerEntryDto()
    {
        Transactions = new List<LedgerTransactionDto>();
    }

    /// <summary>
    ///     The Balance of the Ledger as at the date in the parent LedgerLine
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     The Budget Bucket being tracked.
    ///     The LedgerBucketDto type was intentionally not used here, to prevent the same instance being used between ledger
    ///     lines and the "next reconciliation" mapping at the LedgerBookDto level.
    /// </summary>
    public required string BucketCode { get; init; }

    /// <summary>
    ///     The account in which the Ledger was stored in at the time of the month end reconciliation for this line.
    /// </summary>
    public required string StoredInAccount { get; init; }

    /// <summary>
    ///     Gets or sets the transactions.
    /// </summary>
    public List<LedgerTransactionDto> Transactions { get; init; }
}
