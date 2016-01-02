using System;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching.Data
{
    internal partial class Mapper_MatchingRuleDto_MatchingRule
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_MatchingRuleDto_MatchingRule([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(MatchingRuleDto dto, ref MatchingRule model)
        {
            model = new MatchingRule(this.bucketRepo);
        }

        partial void ToModelPostprocessing(MatchingRuleDto dto, ref MatchingRule model)
        {
            model.BucketCode = dto.BucketCode;
        }

        partial void ToDtoPostprocessing(ref MatchingRuleDto dto, MatchingRule model)
        {
            dto.BucketCode = model.Bucket.Code;
        }
    }
}