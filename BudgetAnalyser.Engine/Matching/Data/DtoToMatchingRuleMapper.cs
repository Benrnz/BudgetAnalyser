namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<MatchingRuleDto, MatchingRule>))]
    public class DtoToMatchingRuleMapper : MagicMapper<MatchingRuleDto, MatchingRule>
    {
    }
}