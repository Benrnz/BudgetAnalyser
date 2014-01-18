using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports
{
    public interface ISpendingGraphAnalyser
    {
        List<KeyValuePair<DateTime, decimal>> ZeroLine { get; }
        List<KeyValuePair<DateTime, decimal>> ActualSpending { get; }
        List<KeyValuePair<DateTime, decimal>> BudgetLine { get; }
        decimal ActualSpendingAxesMinimum { get; }
        decimal NetWorth { get; }
        void Analyse(StatementModel statementModel, BudgetModel budgetModel, IEnumerable<BudgetBucket> buckets, GlobalFilterCriteria criteria);
    }
}