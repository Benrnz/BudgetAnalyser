namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerBucketFactory
    {
        LedgerBucket Build(string bucketCode);
    }
}
