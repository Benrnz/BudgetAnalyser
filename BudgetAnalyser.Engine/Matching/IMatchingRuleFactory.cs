using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    internal interface IMatchingRuleFactory
    {
        MatchingRule CreateNewRule(
            [NotNull] string bucketCode,
            [CanBeNull] string description,
            [CanBeNull] string[] references,
            [CanBeNull] string transactionTypeName,
            [CanBeNull] decimal? amount,
            bool andMatching);

        SingleUseMatchingRule CreateNewSingleUseRule(
            [NotNull] string bucketCode,
            [CanBeNull] string description,
            [CanBeNull] string[] references,
            [CanBeNull] string transactionTypeName,
            [CanBeNull] decimal? amount,
            bool andMatching);

        MatchingRule CreateRuleForPersistence([NotNull] string budgetBucketCode);
        SingleUseMatchingRule CreateSingleUseRuleForPersistence([NotNull] string budgetBucketCode);
    }
}
