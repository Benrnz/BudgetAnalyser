using System;
using System.Linq;
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

            if (statementModel.Transactions.Any(t => t.BudgetBucket == null))
            {
                throw new ArgumentException("There are uncategorised transactions, finish assigning a bucket to all transactions before this running this graph.");
            }

            this.analyser.Analyse(statementModel, criteria);
            var result = this.analyser.Graph;
            this.analyser.Reset();
            return result;
        }
    }
}