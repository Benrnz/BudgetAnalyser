using System.Collections.Generic;
using System.Linq;

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

        public decimal MinimumValue
        {
            get
            {
                var min = PlotsList.Min(p => p.Amount);
                return min < 0 ? min : 0;
            }
        }

        public bool Visible { get; set; }
        internal IList<DatedGraphPlot> PlotsList { get; private set; }
    }
}