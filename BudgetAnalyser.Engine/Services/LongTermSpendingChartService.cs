using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class LongTermSpendingChartService : ILongTermSpendingChartService
    {
        private readonly LongTermSpendingTrendAnalyser analyser;

        public LongTermSpendingChartService([NotNull] LongTermSpendingTrendAnalyser analyser)
        {
            if (analyser == null)
            {
                throw new ArgumentNullException("analyser");
            }

            this.analyser = analyser;
        }

        public GraphData BuildChart(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            this.analyser.Analyse(statementModel, criteria);
            var result = this.analyser.Graph;
            this.analyser.Reset();
            return result;
        }
    }
}