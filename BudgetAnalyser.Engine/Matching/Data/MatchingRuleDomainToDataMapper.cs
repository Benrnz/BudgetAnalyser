namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<MatchingRule, MatchingRuleDto>))]
    public class MatchingRuleDomainToDataMapper : MagicMapper<MatchingRule, MatchingRuleDto>
    {
            // Bucket can be null, while BucketCode will always be the string read from the persistence file.  The currently loaded budget model may not have a bucket
            // that matches that code. So its important to preserve the code.
    }
}