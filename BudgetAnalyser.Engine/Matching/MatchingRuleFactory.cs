using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC]
    internal class MatchingRuleFactory : IMatchingRuleFactory
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public MatchingRuleFactory([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            this.bucketRepo = bucketRepo;
        }

        public MatchingRule CreateRule(string budgetBucketCode)
        {
            return new MatchingRule(this.bucketRepo) { BucketCode = budgetBucketCode };
        }
    }
}