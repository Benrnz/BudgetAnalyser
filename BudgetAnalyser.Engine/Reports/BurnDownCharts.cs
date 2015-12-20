using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A data class used to store compiled results for a series of charts.
    ///     Primarily a collection of <see cref="BurnDownChartAnalyserResult" /> that describes each chart and some overarching
    ///     meta-data for all charts.
    /// </summary>
    public class BurnDownCharts
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BurnDownCharts" /> class.
        /// </summary>
        /// <param name="beginDate">The begin date.</param>
        /// <param name="dateRangeDescription">The date range description.</param>
        /// <param name="listOfCharts">The list of charts.</param>
        public BurnDownCharts(DateTime beginDate, string dateRangeDescription,
            IEnumerable<BurnDownChartAnalyserResult> listOfCharts)
        {
            BeginDate = beginDate;
            DateRangeDescription = dateRangeDescription;
            Charts = listOfCharts;
        }

        /// <summary>
        ///     Gets the begin date for this chart.
        /// </summary>
        public DateTime BeginDate { get; private set; }

        /// <summary>
        ///     Gets a list of compiled result objects ready for binding in the UI.
        /// </summary>
        public IEnumerable<BurnDownChartAnalyserResult> Charts { get; private set; }

        /// <summary>
        ///     Gets the date range description.
        /// </summary>
        public string DateRangeDescription { get; private set; }
    }
}