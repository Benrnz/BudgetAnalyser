namespace BudgetAnalyser.Engine.Widget
{
    public abstract class ProgressBarWidget : Widget
    {
        public double Maximum { get; set; }
        public double Minimum { get; set; }

        public bool ProgressBarVisibility
        {
            get { return true; }
        }

        public double Value { get; set; }
    }
}