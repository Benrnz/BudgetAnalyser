namespace BudgetAnalyser.Engine.Matching.Data
{
    public interface IMatchingRuleDataToDomainMapper
    {
        MatchingRule Map(MatchingRuleDto rule);
    }
}