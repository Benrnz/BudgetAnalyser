using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A class to store the analysis result data for the Overall Performance chart.
    /// </summary>
    public class OverallPerformanceBudgetResult
    {
        /// <summary>
        ///     Gets the analysis list.
        /// </summary>
        public IEnumerable<BucketPerformanceResult> Analyses => AnalysesList;

        internal IList<BucketPerformanceResult> AnalysesList { get; set; }

        /// <summary>
        ///     Gets the average spend per month based on statement transaction data over a period of time.
        ///     This excludes Surplus transactions, these are budgeted expenses only.
        ///     Expected to be negative.
        /// </summary>
        public decimal AverageSpend { get; internal set; }

        /// <summary>
        ///     Gets the average surplus spending per month based on statement transaction data over a period of time.
        /// </summary>
        public decimal AverageSurplus { get; internal set; }

        /// <summary>
        ///     Gets the calculated duration in months.
        /// </summary>
        public int DurationInMonths { get; internal set; }

        /// <summary>
        ///     Gets the calculated overall performance rating.
        /// </summary>
        public decimal OverallPerformance { get; internal set; }

        /// <summary>
        ///     Gets the calculated total budget expenses.
        /// </summary>
        public decimal TotalBudgetExpenses { get; internal set; }

        /// <summary>
        ///     Gets a calculated value indicating whether this analysis spans multiple budgets.
        /// </summary>
        public bool UsesMultipleBudgets { get; internal set; }
    }
}
