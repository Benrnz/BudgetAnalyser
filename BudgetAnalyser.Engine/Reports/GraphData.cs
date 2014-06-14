using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Reports
{
    public class GraphData
    {
        public string GraphName { get; set; }

        public IEnumerable<SeriesData> Series
        {
            get { return SeriesList; }
        }

        public decimal GraphMinimumValue
        {
            get { return SeriesList.Min(s => s.MinimumValue); }
        }

        internal IList<SeriesData> SeriesList { get; set; }
    }
}