using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A data storage class to represent one series on a graph, ie a single line on a graph.
    ///     This class effectively wraps a List of dates and amounts.
    /// </summary>
    public sealed class SeriesData : INotifyPropertyChanged
    {
        private bool doNotUseVisible;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SeriesData" /> class.
        /// </summary>
        public SeriesData()
        {
            PlotsList = new List<DatedGraphPlot>();
            Visible = true;
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The description of the series, typically used in the tool tip for a graph line.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Calculates the smallest value in the data series.
        /// </summary>
        public decimal MinimumValue => PlotsList.Min(p => p.Amount);

        /// <summary>
        ///     Gets the plot points.
        /// </summary>
        public IEnumerable<DatedGraphPlot> Plots => PlotsList;

        internal IList<DatedGraphPlot> PlotsList { get; }

        /// <summary>
        ///     The name of the series, typically used in the graph legend.
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        ///     Gets or sets a boolean to set the visibility of this line on the graph.
        ///     Defaults to true.
        /// </summary>
        public bool Visible
        {
            get { return this.doNotUseVisible; }
            set
            {
                this.doNotUseVisible = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}