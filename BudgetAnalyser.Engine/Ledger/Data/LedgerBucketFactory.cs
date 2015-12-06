using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC]
    public class LedgerBucketFactory : ILedgerBucketFactory
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly IBudgetBucketRepository bucketRepo;

        public LedgerBucketFactory([NotNull] IBudgetBucketRepository bucketRepo, [NotNull] IAccountTypeRepository accountRepo)
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
            Account account = this.accountRepo.GetByKey(accountName);
            return Build(bucketCode, account);
        }

        public LedgerBucket Build(string bucketCode, Account account)
        {
            BudgetBucket bucket = this.bucketRepo.GetByCode(bucketCode);
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

            throw new NotSupportedException($"Unsupported budget bucket {bucketCode} with type {bucket.GetType().Name}, found in ledger book");
        }
    }
}