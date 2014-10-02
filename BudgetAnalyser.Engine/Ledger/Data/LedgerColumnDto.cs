namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Dto for <see cref="LedgerColumn"/>. 
    /// </summary>
    public class LedgerColumnDto
    {
        public string BucketCode { get; set; }

        public string StoredInAccount { get; set; }
    }
}