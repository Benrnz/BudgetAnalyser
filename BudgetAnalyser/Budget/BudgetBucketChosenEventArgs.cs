using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetBucketChosenEventArgs : DialogResponseEventArgs
    {
        public BudgetBucketChosenEventArgs(Guid correlationId, bool canceled) : base(correlationId, canceled)
        {
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

        public BudgetBucket SelectedBucket { get; private set; }
        public AccountType StoreInThisAccount { get; private set; }
    }
}