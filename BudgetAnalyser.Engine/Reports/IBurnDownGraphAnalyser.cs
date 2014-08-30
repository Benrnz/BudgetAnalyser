using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public interface IBurnDownGraphAnalyser
    {
        IEnumerable<KeyValuePair<DateTime, decimal>> ActualSpending { get; }
        decimal ActualSpendingAxesMinimum { get; }
        IEnumerable<KeyValuePair<DateTime, decimal>> BudgetLine { get; }
        IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; }
        IEnumerable<KeyValuePair<DateTime, decimal>> ZeroLine { get; }

        void Analyse(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            DateTime beginDate,
            LedgerBook ledgerBook);
    }
}