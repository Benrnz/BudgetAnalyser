using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Matching;

namespace BudgetAnalyser.Statement
{
    public class TransactionGroupedByBucketViewModel : TransactionGroupedByBucket
    {
        private readonly StatementController statementController;

        public TransactionGroupedByBucketViewModel(
            [NotNull] IEnumerable<Transaction> transactions,
            [NotNull] BudgetBucket groupByThisBucket,
            [NotNull] StatementController statementController)
            : base(transactions, groupByThisBucket)
        {
            if (statementController == null)
            {
                throw new ArgumentNullException("statementController");
            }

            this.statementController = statementController;
        }

        public AppliedRulesController AppliedRulesController
        {
            get { return this.statementController.AppliedRulesController; }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return this.statementController.DeleteTransactionCommand; }
        }

        public ICommand EditTransactionCommand
        {
            get { return this.statementController.EditTransactionCommand; }
        }
    }
}