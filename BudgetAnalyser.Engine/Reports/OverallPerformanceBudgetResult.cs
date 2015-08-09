using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A class to store the analysis result data for the Overall Performance chart.
    /// </summary>
    public class OverallPerformanceBudgetResult
    {
        public IEnumerable<BucketPerformanceResult> Analyses
        {
            get { return AnalysesList; }
        }

        /// <summary>
        ///     Gets the average spend per month based on statement transaction data over a period of time.
        ///     Expected to be negative.
        /// </summary>
        public decimal AverageSpend { get; internal set; }

        /// <summary>
        ///     Gets the average surplus spending per month based on statement transaction data over a period of time.
        /// </summary>
        public decimal AverageSurplus { get; internal set; }

        public int DurationInMonths { get; internal set; }
        public decimal OverallPerformance { get; internal set; }
        public decimal TotalBudgetExpenses { get; internal set; }
        public bool UsesMultipleBudgets { get; internal set; }
        internal IList<BucketPerformanceResult> AnalysesList { get; set; }
    }
}