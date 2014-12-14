namespace BudgetAnalyser.Engine.Budget
{
    public class SurplusBucket : BudgetBucket
    {
        public const string SurplusCode = "SURPLUS";

        public SurplusBucket()
            : base(SurplusCode, "A special bucket to allocate against any discretionary spending.")
        {
        }

        protected SurplusBucket(string bucketCode, string description) : base(bucketCode, description) { }

        public override string TypeDescription
        {
            get { return "Calculated Surplus"; }
        }
    }
}