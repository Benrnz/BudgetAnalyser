using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC]
    internal class MatchingRuleFactory : IMatchingRuleFactory
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public MatchingRuleFactory([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            this.bucketRepo = bucketRepo;
        }

        public MatchingRule CreateNewRule(string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
        {
            return CreateAnyNewRule(CreateRuleForPersistence, bucketCode, description, references, transactionTypeName, amount, andMatching);
        }

        public SingleUseMatchingRule CreateNewSingleUseRule(string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching, int lifetime = 1)
        {
            var rule = CreateAnyNewRule(CreateSingleUseRuleForPersistence, bucketCode, description, references, transactionTypeName, amount, andMatching);
            rule.Lifetime = lifetime;
            return rule;
        }

        public MatchingRule CreateRuleForPersistence(string budgetBucketCode)
        {
            return new MatchingRule(this.bucketRepo) { BucketCode = budgetBucketCode };
        }

        public SingleUseMatchingRule CreateSingleUseRuleForPersistence(string budgetBucketCode)
        {
            return new SingleUseMatchingRule(this.bucketRepo) { BucketCode = budgetBucketCode };
        }

        private T CreateAnyNewRule<T>(Func<string, T> ruleCtor, string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
            where T : MatchingRule
        {
            if (string.IsNullOrEmpty(bucketCode))
            {
                throw new ArgumentNullException(nameof(bucketCode));
            }

            if (references == null)
            {
                throw new ArgumentNullException(nameof(references));
            }

            if (references.Length != 3)
            {
                throw new ArgumentException("The references array is expected to contain 3 elements.");
            }

            T newRule = ruleCtor(bucketCode);
            newRule.Description = description;
            newRule.Reference1 = references[0];
            newRule.Reference2 = references[1];
            newRule.Reference3 = references[2];
            newRule.Amount = amount;
            newRule.TransactionType = transactionTypeName;
            newRule.And = andMatching;
            return newRule;
        }
    }
}