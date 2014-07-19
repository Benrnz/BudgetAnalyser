using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<MatchingRule, MatchingRuleDto>))]
    public class MatchingRuleDomainToDataMapper : BasicMapper<MatchingRule, MatchingRuleDto>
    {
        public override MatchingRuleDto Map([NotNull] MatchingRule source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new MatchingRuleDto
            {
                Amount = source.Amount,
                BucketCode = source.BucketCode, // Its important to use the BucketCode not the Bucket. See below.
                Created = source.Created,
                Description = source.Description,
                LastMatch = source.LastMatch,
                MatchCount = source.MatchCount,
                Reference1 = source.Reference1,
                Reference2 = source.Reference2,
                Reference3 = source.Reference3,
                RuleId = source.RuleId,
                TransactionType = source.TransactionType,
            };

            // Bucket can be null, while BucketCode will always be the string read from the persistence file.  The currently loaded budget model may not have a bucket
            // that matches that code. So its important to preserve the code.
        }
    }
}