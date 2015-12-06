using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerBucketFactory
    {
        LedgerBucket Build(string bucketCode, string accountName);
        LedgerBucket Build(string bucketCode, Account account);
    }
}