﻿using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    internal class OverallPerformanceChartService : IOverallPerformanceChartService
    {
        private readonly OverallPerformanceBudgetAnalyser analyser;

        public OverallPerformanceChartService([NotNull] OverallPerformanceBudgetAnalyser analyser)
        {
            if (analyser is null)
            {
                throw new ArgumentNullException(nameof(analyser));
            }

            this.analyser = analyser;
        }

        public OverallPerformanceBudgetResult BuildChart(StatementModel statementModel, BudgetCollection budgets,
                                                         GlobalFilterCriteria criteria)
        {
            if (statementModel is null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (budgets is null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            if (criteria is null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return this.analyser.Analyse(statementModel, budgets, criteria);
        }
    }
}