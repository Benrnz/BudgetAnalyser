namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingFoodWidget : RemainingBudgetBucketWidget
    {
        public RemainingFoodWidget()
        {
            DetailedText = "Food";
            DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, remaining food budget cannot be calculated.";
            RemainingBudgetToolTip = "Remaining Food budget for period is {0:C}";
            BucketCode = "FOOD";
        }
    }
}