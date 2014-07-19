using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<MatchingRuleDto, MatchingRule>))]
    public class DtoToMatchingRuleMapper : BasicMapper<MatchingRuleDto, MatchingRule>
    {
        private readonly IBudgetBucketRepository bucketRepository;

        public DtoToMatchingRuleMapper([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
        }

        public override MatchingRule Map([NotNull] MatchingRuleDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var domainRule = new MatchingRule(this.bucketRepository)
            {
                Amount = source.Amount,
                BucketCode = source.BucketCode,
                Created = source.Created ?? DateTime.Now,
                Description = source.Description,
                LastMatch = source.LastMatch,
                MatchCount = source.MatchCount,
                Reference1 = source.Reference1,
                Reference2 = source.Reference2,
                Reference3 = source.Reference3,
                RuleId = source.RuleId ?? Guid.Empty,
                TransactionType = source.TransactionType,
            };

            return domainRule;
        }
    }
}