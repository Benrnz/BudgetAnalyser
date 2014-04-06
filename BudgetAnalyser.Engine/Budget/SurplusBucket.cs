using System;

namespace BudgetAnalyser.Engine.Budget
{
    public class SurplusBucket : BudgetBucket
    {
        public const string SurplusCode = "SURPLUS";

        public SurplusBucket()
            : base(SurplusCode, "A special bucket to allocate against any discretionary spending.")
        {
            Id = new Guid("dbaf34f9-5d8d-4984-8303-a022ab49b98a");
        }

        public override string TypeDescription
        {
            get { return "Calculated Surplus"; }
        }
    }
}