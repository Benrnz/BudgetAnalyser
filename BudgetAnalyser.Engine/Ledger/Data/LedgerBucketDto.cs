namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Dto for <see cref="LedgerBucket"/>. 
    /// </summary>
    public class LedgerBucketDto
    {
        public string BucketCode { get; set; }

        public string StoredInAccount { get; set; }
    }
}