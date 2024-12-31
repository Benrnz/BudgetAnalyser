using System;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC]
    internal partial class MapperMatchingRuleDto2MatchingRule(IBudgetBucketRepository bucketRepo)
    {
        private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(MatchingRuleDto dto, ref MatchingRule model)
        {
            model = new MatchingRule(this.bucketRepo);
        }

        partial void ToDtoPostprocessing(ref MatchingRuleDto dto, MatchingRule model)
        {
            dto.BucketCode = model.Bucket.Code;
        }

        partial void ToModelPostprocessing(MatchingRuleDto dto, ref MatchingRule model)
        {
            model.BucketCode = dto.BucketCode;
        }
    }
}
