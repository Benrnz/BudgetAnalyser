using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An interface describing how to maintain <see cref="MatchingRule" />s.
    ///     The collections are owned by the consumer and passed into this service to be manipulated.
    /// </summary>
    public interface ITransactionRuleService : IServiceFoundation
    {
        /// <summary>
        ///     Gets the unique identifer of the currently loaded Matching Rules set.
        /// </summary>
        string RulesStorageKey { get; }

        /// <summary>
        ///     Adds a new rule to the currently loaded set. Will also immediately persist the matching rule set.
        /// </summary>
        /// <param name="rules">The rules collection to add it to.</param>
        /// <param name="rulesGroupedByBucket">The rules grouped by bucket collection.</param>
        /// <param name="ruleToAdd">The rule to add.</param>
        /// <returns>True if the rule was added successfully, otherwise false to indicate the rule already exists.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="rules" /> or <paramref name="rulesGroupedByBucket" /> or
        ///     <paramref name="ruleToAdd" />
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Will be thrown when the service has not yet been initialised by calling
        ///     <see
        ///         cref="PopulateRules(System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.MatchingRule},System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.RulesGroupedByBucket})" />
        ///     or
        ///     <see
        ///         cref="PopulateRules(System.String, System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.MatchingRule},System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.RulesGroupedByBucket})" />
        /// </exception>
        bool AddRule([NotNull] ICollection<MatchingRule> rules, [NotNull] ICollection<RulesGroupedByBucket> rulesGroupedByBucket, [NotNull] MatchingRule ruleToAdd);

        /// <summary>
        ///     Creates a new matching rule.
        /// </summary>
        /// <param name="bucket">The budget bucket to match transactions to.</param>
        /// <param name="description">The description to match. If null, it will not be used to match.</param>
        /// <param name="references">The references to match. If null, it will not be used to match.</param>
        /// <param name="transactionTypeName">Name of the transaction type to match. If null, it will not be used to match.</param>
        /// <param name="amount">The exact amount to match.</param>
        /// <param name="andMatching">
        /// If true, they are matched using an AND operator and all elements must be matched for the rule to match the transaction. Otherwise chosen elements are matched using an OR operator.
        /// </param>
        /// <returns>The new matching rule.</returns>
        MatchingRule CreateNewRule(
            [NotNull] BudgetBucket bucket, 
            [CanBeNull] string description, 
            [NotNull] string[] references, 
            [CanBeNull] string transactionTypeName, 
            [CanBeNull] decimal? amount,
            bool andMatching);

        /// <summary>
        /// Determines whether a rule similar to the input values.
        /// </summary>
        /// <param name="rule">The existing rule to check if the data is similar to it.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="description">The description.</param>
        /// <param name="references">The references.</param>
        /// <param name="transactionTypeName">Name of the transaction type.</param>
        /// <returns>True, if the data is similar to the rule, otherwise false.</returns>
        bool IsRuleSimilar([NotNull] MatchingRule rule, decimal amount, string description, [NotNull] string[] references, string transactionTypeName);

        /// <summary>
        ///     Matches the specified transactions using the provided rules.
        /// </summary>
        /// <param name="transactions">The transactions to scan for matches.</param>
        /// <param name="rules">The rules to use.</param>
        /// <returns>True, if matches were made, otherwise false.</returns>
        bool Match([NotNull] IEnumerable<Transaction> transactions, [NotNull] IEnumerable<MatchingRule> rules);

        /// <summary>
        ///     Initialises the service for use with an empty, default Matching Rules set (for first time use).
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="rulesGroupedByBucket">The rules grouped by bucket.</param>
        void PopulateRules([NotNull] ICollection<MatchingRule> rules, [NotNull] ICollection<RulesGroupedByBucket> rulesGroupedByBucket);

        /// <summary>
        ///     Populates the rules into the provided collections loading the Matching Rule set from persistent storage based on
        ///     the provided <paramref name="storageKey" />.
        /// </summary>
        /// <param name="storageKey">The storage key that uniquely identifies a matching rule set.</param>
        /// <param name="rules">The rules collection to copy the rules into.</param>
        /// <param name="rulesGroupedByBucket">The grouped collection to copy and collate the rules into.</param>
        void PopulateRules([NotNull] string storageKey, [NotNull] ICollection<MatchingRule> rules, [NotNull] ICollection<RulesGroupedByBucket> rulesGroupedByBucket);

        /// <summary>
        ///     Removes from the currently loaded set. Will also immediately persist the matching rule set.
        /// </summary>
        /// <param name="rules">The rules collection to add it to.</param>
        /// <param name="rulesGroupedByBucket">The rules grouped by bucket collection.</param>
        /// <param name="ruleToRemove">The rule to remove.</param>
        /// <returns>True if the rule was removed successfully, otherwise false to indicate the rule didn't exist.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="rules" /> or <paramref name="rulesGroupedByBucket" /> or
        ///     <paramref name="ruleToRemove" />
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Will be thrown when the service has not yet been initialised by calling
        ///     <see
        ///         cref="PopulateRules(System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.MatchingRule},System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.RulesGroupedByBucket})" />
        ///     or
        ///     <see
        ///         cref="PopulateRules(System.String, System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.MatchingRule},System.Collections.Generic.ICollection{BudgetAnalyser.Engine.Matching.RulesGroupedByBucket})" />
        /// </exception>
        bool RemoveRule([NotNull] ICollection<MatchingRule> rules, [NotNull] ICollection<RulesGroupedByBucket> rulesGroupedByBucket, [NotNull] MatchingRule ruleToRemove);

        /// <summary>
        ///     Saves the matching rules set to permenant storage.
        /// </summary>
        /// <param name="rules">The rules to save.</param>
        void SaveRules([NotNull] IEnumerable<MatchingRule> rules);
    }
}