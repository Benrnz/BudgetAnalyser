using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetBucketChosenEventArgs : EventArgs
    {
        public BudgetBucketChosenEventArgs(Guid correlationId, bool canceled)
        {
            CorrelationId = correlationId;
            Canceled = canceled;
        }

        public BudgetBucketChosenEventArgs(Guid correlationId, BudgetBucket bucket) : this(correlationId, false)
        {
            SelectedBucket = bucket;
        }

        public BudgetBucketChosenEventArgs(Guid correlationId, BudgetBucket bucket, AccountType storeInThisAccount)
            : this(correlationId, bucket)
        {
            StoreInThisAccount = storeInThisAccount;
        }

        public bool Canceled { get; private set; }
        public Guid CorrelationId { get; private set; }
        public BudgetBucket SelectedBucket { get; private set; }
        public AccountType StoreInThisAccount { get; private set; }
    }
}