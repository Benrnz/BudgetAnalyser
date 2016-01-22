namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Dto for <see cref="LedgerBucket" />.
    /// </summary>
    public class LedgerBucketDto
    {
        /// <summary>
        ///     Gets or sets the bucket code.
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        ///     Gets or sets the bank account that this ledger bucket is stored in.
        /// </summary>
        public string StoredInAccount { get; set; }
    }
}