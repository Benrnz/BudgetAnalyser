using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     Calculates spending data for one <see cref="BudgetBucket" /> and compares it to budget model data.
    /// </summary>
    [AutoRegisterWithIoC]
    public class BurnDownChartAnalyserResult
    {
        public const string BalanceSeriesName = "Balance Line";
        public const string BudgetSeriesName = "Budget Line";
        public const string ZeroSeriesName = "Zero Line";

        private readonly GraphData graphLines;

        public BurnDownChartAnalyserResult()
        {
            this.graphLines = new GraphData();
        }

        public string ChartTitle { get; internal set; }

        /// <summary>
        ///     Gets the series lines for the burndown graph.  It consists of three lines: Budget, Balance, and zero-line.
        /// </summary>
        public GraphData GraphLines
        {
            get { return this.graphLines; }
        }

        public bool IsCustomAggregateChart { get; internal set; }

        /// <summary>
        ///     The report transactions that decrease the total budgeted amount over time to make up the burn down graph.
        /// </summary>
        public IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; internal set; }
    }
}