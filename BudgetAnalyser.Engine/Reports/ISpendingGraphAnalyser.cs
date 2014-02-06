﻿using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports
{
    public interface ISpendingGraphAnalyser
    {
        List<KeyValuePair<DateTime, decimal>> ActualSpending { get; }
        decimal ActualSpendingAxesMinimum { get; }
        List<KeyValuePair<DateTime, decimal>> BudgetLine { get; }
        decimal NetWorth { get; }
        List<KeyValuePair<DateTime, decimal>> ZeroLine { get; }
        void Analyse(StatementModel statementModel, BudgetModel budgetModel, IEnumerable<BudgetBucket> buckets, GlobalFilterCriteria criteria);
    }
}