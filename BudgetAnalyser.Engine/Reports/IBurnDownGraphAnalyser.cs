using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public interface IBurnDownGraphAnalyser
    {
        GraphData GraphLines { get; }
        IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; }

        void Analyse(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            DateTime beginDate,
            LedgerBook ledgerBook);
    }
}