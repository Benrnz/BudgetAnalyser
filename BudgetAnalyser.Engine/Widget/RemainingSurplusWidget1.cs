namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingSurplusWidget1 : RemainingBudgetBucketWidget
    {
        public RemainingSurplusWidget1()
        {
            DetailedText = "Surplus";
            DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, surplus cannot be calculated.";
            RemainingBudgetToolTip = "Remaining Surplus for period is {0:C}";
            BucketCode = "SURPLUS";
        }

        protected override decimal MonthlyBudgetAmount()
        {
            return Budget.Model.Surplus;
        }
    }
}