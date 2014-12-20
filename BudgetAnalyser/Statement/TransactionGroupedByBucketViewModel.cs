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

        public TransactionGroupedByBucketViewModel(TransactionGroupedByBucket baseLine, StatementController statementController)
            : base(baseLine.Transactions, baseLine.Bucket)
        {
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