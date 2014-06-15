using System;

namespace BudgetAnalyser.Budget
{
    public class BudgetBucketChosenEventArgs : EventArgs
    {
        public BudgetBucketChosenEventArgs(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; private set; }
    }
}
