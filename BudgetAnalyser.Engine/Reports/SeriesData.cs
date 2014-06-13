using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    public class SeriesData
    {
        public SeriesData()
        {
            PlotsList = new List<DatedGraphPlot>();
            Visible = true;
        }

        public IEnumerable<DatedGraphPlot> Plots
        {
            get { return PlotsList; }
        }

        public string SeriesName { get; set; }

        public string Description { get; set; }

        public bool Visible { get; set; }
        internal IList<DatedGraphPlot> PlotsList { get; private set; }
    }
}