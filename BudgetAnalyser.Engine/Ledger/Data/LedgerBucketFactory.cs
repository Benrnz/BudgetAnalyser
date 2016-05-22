using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A factory to create <see cref="LedgerBucket" />s from minimaly persisted storage data.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.Data.ILedgerBucketFactory" />
    [AutoRegisterWithIoC]
    internal class LedgerBucketFactory : ILedgerBucketFactory
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly IBudgetBucketRepository bucketRepo;

        public LedgerBucketFactory([NotNull] IBudgetBucketRepository bucketRepo,
                                   [NotNull] IAccountTypeRepository accountRepo)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            if (accountRepo == null)
            {
                throw new ArgumentNullException(nameof(accountRepo));
            }

            this.bucketRepo = bucketRepo;
            this.accountRepo = accountRepo;
        }

        public LedgerBucket Build(string bucketCode, string accountName)
        {
            var account = this.accountRepo.GetByKey(accountName);
            return Build(bucketCode, account);
        }

        public LedgerBucket Build(string bucketCode, Account account)
        {
            var bucket = this.bucketRepo.GetByCode(bucketCode);
            if (bucket is SavedUpForExpenseBucket)
            {
                return new SavedUpForLedger { BudgetBucket = bucket, StoredInAccount = account };
            }

            if (bucket is SpentMonthlyExpenseBucket)
            {
                return new SpentMonthlyLedger { BudgetBucket = bucket, StoredInAccount = account };
            }

            if (bucket is SavingsCommitmentBucket)
            {
                return new SavedUpForLedger { BudgetBucket = bucket, StoredInAccount = account };
            }

            throw new NotSupportedException(
                $"Unsupported budget bucket {bucketCode} with type {bucket.GetType().Name}, found in ledger book");
        }
    }
}