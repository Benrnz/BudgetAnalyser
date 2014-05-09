using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widget
{
    public abstract class ProgressBarWidget : Widget
    {
        private double doNotUseMaximum;
        private double doNotUseMinimum;
        private double doNotUseValue;

        public double Maximum
        {
            get { return this.doNotUseMaximum; }
            set
            {
                this.doNotUseMaximum = value;
                OnPropertyChanged();
            }
        }

        public double Minimum
        {
            get { return this.doNotUseMinimum; }
            set
            {
                this.doNotUseMinimum = value;
                OnPropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for Data binding")]
        public bool ProgressBarVisibility
        {
            get { return true; }
        }

        public double Value
        {
            get { return this.doNotUseValue; }
            set
            {
                this.doNotUseValue = value;
                OnPropertyChanged();
            }
        }
    }
}