namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class MatchingRuleDomainToDataMapper : IMatchingRuleDomainToDataMapper
    {
        public DataMatchingRule Map(MatchingRule rule)
        {
            return new DataMatchingRule
            {
                Amount = rule.Amount,
                BucketCode = rule.BucketCode,
                Description = rule.Description,
                LastMatch = rule.LastMatch,
                MatchCount = rule.MatchCount,
                Reference1 = rule.Reference1,
                Reference2 = rule.Reference2,
                Reference3 = rule.Reference3,
                TransactionType = rule.TransactionType,
            };
        }
    }
}