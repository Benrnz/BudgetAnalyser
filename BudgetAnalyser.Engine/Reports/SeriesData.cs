using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    public sealed class SeriesData : INotifyPropertyChanged
    {
        private bool doNotUseVisible;

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

        public bool Visible
        {
            get { return this.doNotUseVisible; }
            set
            {
                this.doNotUseVisible = value;
                OnPropertyChanged();
            }
        }

        internal IList<DatedGraphPlot> PlotsList { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}