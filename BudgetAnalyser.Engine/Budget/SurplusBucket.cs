namespace BudgetAnalyser.Engine.Budget
{
    public class SurplusBucket : BudgetBucket
    {
        public const string SurplusCode = "SURPLUS";

        public SurplusBucket()
            : base(SurplusCode, "A special bucket to allocate against any discretionary spending.")
        {
        }

        public override string TypeDescription
        {
            get { return "Calculated Surplus"; }
        }
    }
}