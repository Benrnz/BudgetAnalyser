namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<MatchingRule, MatchingRuleDto>))]
    public class MatchingRuleDomainToDataMapper : MagicMapper<MatchingRule, MatchingRuleDto>
    {
    }
}