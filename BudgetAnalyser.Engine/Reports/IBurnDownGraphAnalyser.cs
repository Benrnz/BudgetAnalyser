using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public interface IBurnDownGraphAnalyser
    {
        List<KeyValuePair<DateTime, decimal>> ActualSpending { get; }
        decimal ActualSpendingAxesMinimum { get; }
        List<KeyValuePair<DateTime, decimal>> BudgetLine { get; }
        decimal NetWorth { get; }
        List<KeyValuePair<DateTime, decimal>> ZeroLine { get; }
        void Analyse(
            StatementModel statementModel, 
            BudgetModel budgetModel, 
            IEnumerable<BudgetBucket> buckets, 
            DateTime beginDate,
            LedgerBook ledgerBook);
    }
}