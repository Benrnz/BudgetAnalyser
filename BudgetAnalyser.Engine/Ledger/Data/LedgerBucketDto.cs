namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerBucket" />.
/// </summary>
public class LedgerBucketDto
{
    /// <summary>
    ///     Gets or sets the bucket code.
    /// </summary>
    public required string BucketCode { get; init; }

    /// <summary>
    ///     Gets or sets the bank account that this ledger bucket is stored in.
    /// </summary>
    public required string StoredInAccount { get; init; }
}
