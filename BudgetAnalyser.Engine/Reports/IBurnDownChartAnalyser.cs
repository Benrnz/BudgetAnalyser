using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    internal interface IBurnDownChartAnalyser
    {
        /// <summary>
        ///     Analyse the actual spending over a month as a burn down of the available budget.
        ///     The available budget is either the <paramref name="ledgerBook" /> balance of the ledger or if not tracked in the
        ///     ledger, then
        ///     the budgeted amount from the <paramref name="budgetModel" />.
        /// </summary>
        BurnDownChartAnalyserResult Analyse(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            LedgerBook ledgerBook,
            DateTime beginDate);
    }
}