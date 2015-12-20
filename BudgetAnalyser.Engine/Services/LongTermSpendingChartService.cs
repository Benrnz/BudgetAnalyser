using System;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    internal class LongTermSpendingChartService : ILongTermSpendingChartService
    {
        private readonly LongTermSpendingTrendAnalyser analyser;

        public LongTermSpendingChartService([NotNull] LongTermSpendingTrendAnalyser analyser)
        {
            if (analyser == null)
            {
                throw new ArgumentNullException(nameof(analyser));
            }

            this.analyser = analyser;
        }

        public GraphData BuildChart(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            this.analyser.Analyse(statementModel, criteria);
            GraphData result = this.analyser.Graph;
            this.analyser.Reset();
            return result;
        }
    }
}