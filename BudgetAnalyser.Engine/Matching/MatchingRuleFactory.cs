using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

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

        public SingleUseMatchingRule CreateNewSingleUseRule(string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
        {
            var rule = CreateAnyNewRule(CreateSingleUseRuleForPersistence, bucketCode, description, references, transactionTypeName, amount, andMatching);
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

        private static T CreateAnyNewRule<T>(Func<string, T> ruleCtor, string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
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

            if (references.Length > 3)
            {
                throw new ArgumentException("The references array is expected to contain 3 elements.");
            }

            var adjustedReferences = new List<string>();
            if (references.Length < 3)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (i <= references.GetUpperBound(0))
                    {
                        adjustedReferences.Add(references[i]);
                    }
                    else
                    {
                        adjustedReferences.Add(null);
                    }
                }
            }
            else
            {
                adjustedReferences = references.ToList();
            }

            var newRule = ruleCtor(bucketCode);
            newRule.Description = description;
            newRule.Reference1 = adjustedReferences[0];
            newRule.Reference2 = adjustedReferences[1];
            newRule.Reference3 = adjustedReferences[2];
            newRule.Amount = amount;
            newRule.TransactionType = transactionTypeName;
            newRule.And = andMatching;
            return newRule;
        }
    }
}