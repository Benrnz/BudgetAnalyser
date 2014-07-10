using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BucketBucketRepoAlwaysFind : InMemoryBudgetBucketRepository
    {
        public override BudgetBucket GetByCode(string code)
        {
            if (code.StartsWith("I"))
            {
                return GetOrCreateNew(code, () => new IncomeBudgetBucket(code, code));
            }

            return GetOrCreateNew(code, () => new SavedUpForExpenseBucket(code, code));
        }

        public override void Initialise(BudgetCollection budgetCollectionModel)
        {
            InitialiseMandatorySpecialBuckets();
        }
    }
}
