namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingFuelWidget : RemainingBudgetBucketWidget
    {
        public RemainingFuelWidget()
        {
            DetailedText = "Fuel";
            DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, remaining fuel budget cannot be calculated.";
            RemainingBudgetToolTip = "Remaining Fuel budget for period is {0:C}";
            BucketCode = "FUEL";
        }
    }
}