using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleFactory
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
            bool andMatching,
            int lifetime = 1);

        MatchingRule CreateRuleForPersistence([NotNull] string budgetBucketCode);
        SingleUseMatchingRule CreateSingleUseRuleForPersistence([NotNull] string budgetBucketCode);
    }
}