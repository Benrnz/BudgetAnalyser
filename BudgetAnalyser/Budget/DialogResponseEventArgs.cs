using System;

namespace BudgetAnalyser.Budget
{
    public class DialogResponseEventArgs : EventArgs
    {
        public DialogResponseEventArgs(Guid correlationId, bool canceled)
        {
            CorrelationId = correlationId;
            Canceled = canceled;
        }

        public bool Canceled { get; private set; }
        public Guid CorrelationId { get; private set; }
    }
}
