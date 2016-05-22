using System.Collections.Generic;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An interface describing how to maintain <see cref="MatchingRule" />s.
    ///     The collections are owned by the consumer and passed into this service to be manipulated.
    ///     This service is designed to be stateful.
    /// </summary>
    public interface ITransactionRuleService : INotifyDatabaseChanges, IServiceFoundation
    {
        /// <summary>
        ///     Gets the matching rules for data binding in the UI.
        /// </summary>
        IEnumerable<MatchingRule> MatchingRules { get; }

        /// <summary>
        ///     Gets the matching rules grouped by bucket.
        /// </summary>
        IEnumerable<RulesGroupedByBucket> MatchingRulesGroupedByBucket { get; }

        /// <summary>
        ///     Creates a new matching rule.
        /// </summary>
        /// <param name="bucketCode">The budget bucket to match transactions to.</param>
        /// <param name="description">The description to match. If null, it will not be used to match.</param>
        /// <param name="references">The references to match. If null, it will not be used to match.</param>
        /// <param name="transactionTypeName">Name of the transaction type to match. If null, it will not be used to match.</param>
        /// <param name="amount">The exact amount to match.</param>
        /// <param name="andMatching">
        ///     If true, they are matched using an AND operator and all elements must be matched for the rule to match the
        ///     transaction. Otherwise chosen elements are matched using an OR operator.
        /// </param>
        /// <returns>The new matching rule.</returns>
        MatchingRule CreateNewRule(
            [NotNull] string bucketCode,
            [CanBeNull] string description,
            [NotNull] string[] references,
            [CanBeNull] string transactionTypeName,
            [CanBeNull] decimal? amount,
            bool andMatching);

        /// <summary>
        ///     Creates a new single use matching rule. (One that will be deleted after it is used to make a match).
        /// </summary>
        /// <param name="bucketCode">The budget bucket code to match transactions to.</param>
        /// <param name="description">The description to match. If null, it will not be used to match.</param>
        /// <param name="references">The references to match. If null, it will not be used to match.</param>
        /// <param name="transactionTypeName">Name of the transaction type to match. If null, it will not be used to match.</param>
        /// <param name="amount">The exact amount to match.</param>
        /// <param name="andMatching">
        ///     If true, they are matched using an AND operator and all elements must be matched for the rule to match the
        ///     transaction. Otherwise chosen elements are matched using an OR operator.
        /// </param>
        /// <returns>The new matching rule.</returns>
        SingleUseMatchingRule CreateNewSingleUseRule(
            [NotNull] string bucketCode,
            [CanBeNull] string description,
            [NotNull] string[] references,
            [CanBeNull] string transactionTypeName,
            [CanBeNull] decimal? amount,
            bool andMatching);

        /// <summary>
        ///     Determines whether a rule similar to the input values.
        /// </summary>
        /// <param name="rule">The existing rule to check if the data is similar to it.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="description">The description.</param>
        /// <param name="references">The references.</param>
        /// <param name="transactionTypeName">Name of the transaction type.</param>
        /// <returns>True, if the data is similar to the rule, otherwise false.</returns>
        bool IsRuleSimilar(
            [NotNull] SimilarMatchedRule rule,
            [NotNull] DecimalCriteria amount,
            [NotNull] StringCriteria description,
            [NotNull] StringCriteria[] references,
            [NotNull] StringCriteria transactionTypeName);

        /// <summary>
        ///     Matches the specified transactions using the provided rules.
        /// </summary>
        /// <param name="transactions">The transactions to scan for matches.</param>
        /// <returns>True, if matches were made, otherwise false.</returns>
        bool Match([NotNull] IEnumerable<Transaction> transactions);

        /// <summary>
        ///     Removes from the currently loaded set. Will also immediately persist the matching rule set.
        /// </summary>
        /// <param name="ruleToRemove">The rule to remove.</param>
        /// <returns>True if the rule was removed successfully, otherwise false to indicate the rule didn't exist.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="ruleToRemove" />
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Will be thrown when the service has not yet been initialised by calling
        ///     <see cref="ISupportsModelPersistence.LoadAsync" />
        /// </exception>
        bool RemoveRule([NotNull] MatchingRule ruleToRemove);
    }
}