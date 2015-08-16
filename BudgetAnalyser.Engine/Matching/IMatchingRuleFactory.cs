namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleFactory
    {
        MatchingRule CreateRule(string budgetBucketCode);
    }
}