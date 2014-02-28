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
                this.doNotUseTransaction = value;
                OriginalHash = value.GetHashCode();
            }
        }

        public int OriginalHash { get; private set; }

        public bool HasChanged
        {
            get { return OriginalHash != Transaction.GetHashCode(); }
        }
    }
}
