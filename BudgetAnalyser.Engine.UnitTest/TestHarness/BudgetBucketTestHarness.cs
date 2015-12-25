using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class BudgetBucketTestHarness : BudgetBucket
    {
        public BudgetBucketTestHarness()
        {
        }

        public BudgetBucketTestHarness(string code, string name) : base(code, name)
        {
        }
    }
}