using System.Collections.Generic;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BucketBucketRepoAlwaysFind : InMemoryBudgetBucketRepository
    {
        public BucketBucketRepoAlwaysFind() : base(new BasicMapper<BudgetBucketDto, BudgetBucket>())
        {
        }

        public override BudgetBucket GetByCode(string code)
        {
            if (code.StartsWith("I"))
            {
                return GetOrCreateNew(code, () => new IncomeBudgetBucket(code, code));
            }

            return GetOrCreateNew(code, () => new SavedUpForExpenseBucket(code, code));
        }

        public override void Initialise(IEnumerable<BudgetBucketDto> buckets)
        {
            InitialiseMandatorySpecialBuckets();
        }
    }
}
