using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    internal interface ILedgerBucketFactory
    {
        LedgerBucket Build(string bucketCode, string accountName);
        LedgerBucket Build(string bucketCode, Account account);
    }
}