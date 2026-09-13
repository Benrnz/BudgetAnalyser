using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A factory to create <see cref="LedgerBucket" />s from minimally persisted storage data.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Ledger.Data.ILedgerBucketFactory" />
[AutoRegisterWithIoC]
internal class LedgerBucketFactory(IBudgetBucketRepository bucketRepo, IAccountTypeRepository accountRepo) : ILedgerBucketFactory
{
    private readonly IAccountTypeRepository accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

    public LedgerBucket Build(string bucketCode, string accountName)
    {
        var account = this.accountRepo.GetByKey(accountName) ?? throw new CorruptedLedgerBookException($"Provided account '${accountName}' does not exist.");
        return Build(bucketCode, account);
    }

    public LedgerBucket Build(string bucketCode, Account account)
    {
        var bucket = this.bucketRepo.GetByCode(bucketCode);
        if (bucket is SavedUpForExpenseBucket)
        {
            return new SavedUpForLedger { BudgetBucket = bucket, StoredInAccount = account };
        }

        if (bucket is SpentPerPeriodExpenseBucket)
        {
            return new SpentPerPeriodLedger { BudgetBucket = bucket, StoredInAccount = account };
        }

        throw new CorruptedLedgerBookException($"Unsupported budget bucket '{bucketCode}', found in ledger book");
    }
}
