namespace BudgetAnalyser.Engine.Matching.Data
{
    public interface IMatchingRuleDomainToDataMapper
    {
        MatchingRuleDto Map(MatchingRule rule);
    }
}