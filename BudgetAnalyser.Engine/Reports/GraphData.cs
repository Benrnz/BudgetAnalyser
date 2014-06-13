using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    public class GraphData
    {
        public string GraphName { get; set; }

        public IEnumerable<SeriesData> Series
        {
            get { return SeriesList; }
        }

        internal IList<SeriesData> SeriesList { get; set; }
    }
}