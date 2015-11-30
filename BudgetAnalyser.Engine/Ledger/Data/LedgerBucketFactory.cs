using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC]
    public class LedgerBucketFactory : ILedgerBucketFactory
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public LedgerBucketFactory([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            this.bucketRepo = bucketRepo;
        }

        public LedgerBucket Build(string bucketCode)
        {
            var bucket = this.bucketRepo.GetByCode(bucketCode);
            if (bucket is SavedUpForExpenseBucket)
            {
                return new SavedUpForLedger { BudgetBucket = bucket };
            }

            if (bucket is SpentMonthlyExpenseBucket)
            {
                return new SpentMonthlyLedger { BudgetBucket = bucket };
            }

            if (bucket is SavingsCommitmentBucket)
            {
                return new SavedUpForLedger { BudgetBucket = bucket };
            }

            throw new NotSupportedException($"Unsupported budget bucket {bucketCode} with type {bucket.GetType().Name}, found in ledger book");
        }
    }
}