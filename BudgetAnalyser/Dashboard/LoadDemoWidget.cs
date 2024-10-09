using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Dashboard
{
    public class LoadDemoWidget : Widget
    {
        public LoadDemoWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new Type[] { };
            DetailedText = "Load Demo";
            ImageResourceName = "SmileyImage";
            Clickable = true;
            Sequence = 25;
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            Enabled = true;
            ToolTip = "Load a demo to show the basic features of Budget Analyser.";
        }
    }
}