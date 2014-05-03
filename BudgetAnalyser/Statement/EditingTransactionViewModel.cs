using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Statement
{
    public class EditingTransactionViewModel
    {
        private Transaction doNotUseTransaction;

        public Transaction Transaction
        {
            get { return this.doNotUseTransaction; }
            set
            {
                if (value == null)
                {
                    OriginalHash = 0;
                }
                else
                {
                    OriginalHash = value.GetEqualityHashCode();
                }

                this.doNotUseTransaction = value;
            }
        }

        public int OriginalHash { get; private set; }

        public bool HasChanged
        {
            get { return OriginalHash != Transaction.GetEqualityHashCode(); }
        }
    }
}
