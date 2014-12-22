using System.Windows.Input;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Matching;

namespace BudgetAnalyser.Statement
{
    public class TransactionGroupedByBucketViewModel : TransactionGroupedByBucket
    {
        public TransactionGroupedByBucketViewModel(TransactionGroupedByBucket baseLine, IUiContext uiContext)
            : base(baseLine.Transactions, baseLine.Bucket)
        {
            DeleteTransactionCommand = uiContext.StatementController.DeleteTransactionCommand;
            EditTransactionCommand = uiContext.StatementController.EditTransactionCommand;
            AppliedRulesController = uiContext.StatementController.AppliedRulesController;
        }

        public AppliedRulesController AppliedRulesController { get; private set; }
        public ICommand DeleteTransactionCommand { get; private set; }
        public ICommand EditTransactionCommand { get; private set; }
    }
}