namespace BudgetAnalyser.Engine.Matching;

internal interface IMatchingRuleFactory
{
    MatchingRule CreateNewRule(
        string bucketCode,
        string? description,
        string?[] references,
        string? transactionTypeName,
        decimal? amount,
        bool andMatching);

    SingleUseMatchingRule CreateNewSingleUseRule(
        string bucketCode,
        string? description,
        string?[] references,
        string? transactionTypeName,
        decimal? amount,
        bool andMatching);
}
