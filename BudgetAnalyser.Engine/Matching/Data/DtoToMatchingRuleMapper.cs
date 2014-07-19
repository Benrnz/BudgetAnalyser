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

        public override MatchingRule Map([NotNull] MatchingRuleDto rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            var domainRule = new MatchingRule(this.bucketRepository)
            {
                Amount = rule.Amount,
                BucketCode = rule.BucketCode,
                Created = rule.Created ?? DateTime.Now,
                Description = rule.Description,
                LastMatch = rule.LastMatch,
                MatchCount = rule.MatchCount,
                Reference1 = rule.Reference1,
                Reference2 = rule.Reference2,
                Reference3 = rule.Reference3,
                RuleId = rule.RuleId ?? Guid.Empty,
                TransactionType = rule.TransactionType,
            };

            return domainRule;
        }
    }
}