using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetBucketChosenEventArgs : DialogResponseEventArgs
    {
        public BudgetBucketChosenEventArgs(Guid correlationId, bool canceled) : base(correlationId, canceled)
        {
        }

        public BudgetBucketChosenEventArgs(Guid correlationId, BudgetBucket? bucket) : this(correlationId, false)
        {
            SelectedBucket = bucket;
        }

        public BudgetBucketChosenEventArgs(Guid correlationId, BudgetBucket? bucket, Account storeInThisAccount)
            : this(correlationId, bucket)
        {
            StoreInThisAccount = storeInThisAccount;
        }

        public BudgetBucket? SelectedBucket { get; private set; }
        public Account StoreInThisAccount { get; private set; }
    }
}