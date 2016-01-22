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
        /// <summary>
        ///     A constant for the balance series name
        /// </summary>
        public const string BalanceSeriesName = "Balance Line";

        /// <summary>
        ///     A constant for the budget series name
        /// </summary>
        public const string BudgetSeriesName = "Budget Line";

        /// <summary>
        ///     A constant for the zero series name
        /// </summary>
        public const string ZeroSeriesName = "Zero Line";

        /// <summary>
        ///     Initializes a new instance of the <see cref="BurnDownChartAnalyserResult" /> class.
        /// </summary>
        public BurnDownChartAnalyserResult()
        {
            GraphLines = new GraphData();
        }

        /// <summary>
        ///     Gets the chart title.
        /// </summary>
        public string ChartTitle { get; internal set; }

        /// <summary>
        ///     Gets the series lines for the burndown graph.  It consists of three lines: Budget, Balance, and zero-line.
        /// </summary>
        public GraphData GraphLines { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is custom aggregate chart.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is custom aggregate chart; otherwise, <c>false</c>.
        /// </value>
        public bool IsCustomAggregateChart { get; internal set; }

        /// <summary>
        ///     The report transactions that decrease the total budgeted amount over time to make up the burn down graph.
        /// </summary>
        public IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; internal set; }
    }
}