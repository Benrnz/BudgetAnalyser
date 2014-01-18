namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleDomainToDataMapper
    {
        DataMatchingRule Map(MatchingRule rule);
    }
}