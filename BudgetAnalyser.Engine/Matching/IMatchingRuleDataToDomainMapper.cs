namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleDataToDomainMapper
    {
        MatchingRule Map(DataMatchingRule rule);
    }
}