using System;
using System.Collections.Generic;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class BurnDownChartBuilderResults
    {
        public BurnDownChartBuilderResults(DateTime beginDate, string dateRangeDescription, IEnumerable<BucketBurnDownController> listOfCharts)
        {
            this.BeginDate = beginDate;
            this.DateRangeDescription = dateRangeDescription;
            this.Charts = listOfCharts;
        }

        public DateTime BeginDate { get; private set; }
        public IEnumerable<BucketBurnDownController> Charts { get; private set; }

        public string DateRangeDescription { get; private set; }
    }
}