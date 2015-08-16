using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    public abstract class ProgressBarWidget : Widget
    {
        private bool doNotUseEnabled;
        private double doNotUseMaximum;
        private double doNotUseMinimum;
        private bool doNotUseProgressBarVisibility;
        private double doNotUseValue;

        public override bool Enabled
        {
            get { return this.doNotUseEnabled; }
            protected set
            {
                this.doNotUseEnabled = value;
                if (!this.doNotUseEnabled)
                {
                    Value = 0;
                    ProgressBarVisibility = false;
                }
                else
                {
                    ProgressBarVisibility = true;
                }

                OnPropertyChanged();
            }
        }

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
            get { return this.doNotUseProgressBarVisibility; }
            protected set
            {
                this.doNotUseProgressBarVisibility = value;
                OnPropertyChanged();
            }
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