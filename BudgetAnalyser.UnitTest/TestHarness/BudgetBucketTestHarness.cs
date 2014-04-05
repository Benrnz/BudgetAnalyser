using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BudgetBucketTestHarness : BudgetBucket
    {
        public BudgetBucketTestHarness() : base()
        {
        }

        public BudgetBucketTestHarness(string code, string name) : base(code, name)
        {
        }
    }
}
