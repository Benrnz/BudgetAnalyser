namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILegderBucketFactory
    {
        LedgerBucket Build(string bucketCode);
    }
}
