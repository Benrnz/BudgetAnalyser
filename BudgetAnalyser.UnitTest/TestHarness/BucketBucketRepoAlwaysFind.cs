using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BucketBucketRepoAlwaysFind : InMemoryBudgetBucketRepository
    {
        private readonly string projectPrefix;

        public BucketBucketRepoAlwaysFind() : base(new BasicMapperFake<BudgetBucketDto, BudgetBucket>())
        {
            SurplusBucket = new SurplusBucket();
            this.projectPrefix = string.Format(FixedBudgetProjectBucket.ProjectCodeTemplateWithPrefix, string.Empty);
        }

        public override BudgetBucket GetByCode(string code)
        {
            if (code.StartsWith("INCOME"))
            {
                return GetOrCreateNew(code, () => new IncomeBudgetBucket(code, code));
            }

            if (string.CompareOrdinal(code, SurplusBucket.Code) == 0)
            {
                return SurplusBucket;
            }

            if (code.StartsWith(this.projectPrefix))
            {
                return GetOrCreateNew(code, () => new FixedBudgetProjectBucket(code, code, 100000M));
            }

            return GetOrCreateNew(code, () => new SavedUpForExpenseBucket(code, code));
        }

        public override void Initialise(IEnumerable<BudgetBucketDto> buckets)
        {
            InitialiseMandatorySpecialBuckets();
        }

        public BucketBucketRepoAlwaysFind WithSurplusAdded()
        {
            GetOrCreateNew(SurplusBucket.Code, () => SurplusBucket);
            return this;
        }
    }
}