using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class TransactionRuleService : ITransactionRuleService
    {
        private readonly ILogger logger;
        private readonly IMatchmaker matchmaker;
        private readonly IMatchingRuleFactory ruleFactory;
        private readonly IMatchingRuleRepository ruleRepository;

        public TransactionRuleService(
            [NotNull] IMatchingRuleRepository ruleRepository,
            [NotNull] ILogger logger,
            [NotNull] IMatchmaker matchmaker,
            [NotNull] IMatchingRuleFactory ruleFactory)
        {
            if (ruleRepository == null)
            {
                throw new ArgumentNullException("ruleRepository");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (matchmaker == null)
            {
                throw new ArgumentNullException("matchmaker");
            }

            if (ruleFactory == null)
            {
                throw new ArgumentNullException("ruleFactory");
            }

            this.ruleRepository = ruleRepository;
            this.logger = logger;
            this.matchmaker = matchmaker;
            this.ruleFactory = ruleFactory;
        }

        public string RulesStorageKey { get; private set; }

        public bool AddRule(ICollection<MatchingRule> rules, ICollection<RulesGroupedByBucket> rulesGroupedByBucket, MatchingRule ruleToAdd)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            if (rulesGroupedByBucket == null)
            {
                throw new ArgumentNullException("rulesGroupedByBucket");
            }
            if (ruleToAdd == null)
            {
                throw new ArgumentNullException("ruleToAdd");
            }
            if (string.IsNullOrWhiteSpace(RulesStorageKey))
            {
                throw new InvalidOperationException("Unable to add a matching rule at this time, the service has not yet loaded a matching rule set.");
            }

            RulesGroupedByBucket existingGroup = rulesGroupedByBucket.FirstOrDefault(group => group.Bucket == ruleToAdd.Bucket);
            if (existingGroup == null)
            {
                var addNewGroup = new RulesGroupedByBucket(ruleToAdd.Bucket, new[] { ruleToAdd });
                rulesGroupedByBucket.Add(addNewGroup);
                rules.Add(ruleToAdd);
            }
            else
            {
                if (existingGroup.Rules.Contains(ruleToAdd))
                {
                    this.logger.LogWarning(l => "Attempt to add new rule failed. Rule already exists in Grouped collection. " + ruleToAdd);
                    return false;
                }
                existingGroup.Rules.Add(ruleToAdd);
                if (rules.Contains(ruleToAdd))
                {
                    this.logger.LogWarning(l => "Attempt to add new rule failed. Rule already exists in main collection. " + ruleToAdd);
                    return false;
                }
                rules.Add(ruleToAdd);
            }

            SaveRules(rules);
            this.logger.LogInfo(_ => "Matching Rule Added: " + ruleToAdd);

            return true;
        }

        public MatchingRule CreateNewRule(BudgetBucket bucket, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            if (references == null)
            {
                throw new ArgumentNullException("references");
            }

            if (references.Length != 3)
            {
                throw new ArgumentException("The references array is expected to contain 3 elements.");
            }

            MatchingRule newRule = this.ruleFactory.CreateRule(bucket.Code);
            newRule.Description = description;
            newRule.Reference1 = references[0];
            newRule.Reference2 = references[1];
            newRule.Reference3 = references[2];
            newRule.Amount = amount;
            newRule.TransactionType = transactionTypeName;
            newRule.And = andMatching;
            return newRule;
        }

        public bool IsRuleSimilar(MatchingRule rule, decimal amount, string description, string[] references, string transactionTypeName)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            if (references == null)
            {
                throw new ArgumentNullException("references");
            }

            return amount == rule.Amount
                   || IsEqualButNotBlank(description, rule.Description)
                   || IsEqualButNotBlank(references[0], rule.Reference1)
                   || IsEqualButNotBlank(references[1], rule.Reference2)
                   || IsEqualButNotBlank(references[2], rule.Reference3)
                   || IsEqualButNotBlank(transactionTypeName, rule.TransactionType);
        }

        public bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules)
        {
            return this.matchmaker.Match(transactions, rules);
        }

        public void PopulateRules(ICollection<MatchingRule> rules, ICollection<RulesGroupedByBucket> rulesGroupedByBucket)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            if (rulesGroupedByBucket == null)
            {
                throw new ArgumentNullException("rulesGroupedByBucket");
            }
            PopulateRules(BuildDefaultFileName(), rules, rulesGroupedByBucket);
        }

        public void PopulateRules(string storageKey, ICollection<MatchingRule> rules, ICollection<RulesGroupedByBucket> rulesGroupedByBucket)
        {
            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            if (rulesGroupedByBucket == null)
            {
                throw new ArgumentNullException("rulesGroupedByBucket");
            }
            rules.Clear();
            rulesGroupedByBucket.Clear();
            RulesStorageKey = storageKey;
            try
            {
                List<MatchingRule> repoRules = this.ruleRepository.LoadRules(RulesStorageKey)
                    .OrderBy(r => r.Description)
                    .ToList();

                foreach (MatchingRule rule in repoRules)
                {
                    rules.Add(rule);
                }

                IEnumerable<RulesGroupedByBucket> grouped = repoRules.GroupBy(rule => rule.Bucket)
                    .Where(group => group.Key != null)
                    // this is to prevent showing rules that have a bucket code not currently in the current budget model. Happens when loading the demo or empty budget model.
                    .Select(group => new RulesGroupedByBucket(group.Key, group))
                    .OrderBy(group => group.Bucket.Code);

                foreach (RulesGroupedByBucket groupedByBucket in grouped)
                {
                    rulesGroupedByBucket.Add(groupedByBucket);
                }
            }
            catch (FileNotFoundException)
            {
                // If file not found occurs here, assume this is the first time the app has run, and create a new one.
                RulesStorageKey = BuildDefaultFileName();
                this.ruleRepository.SaveRules(new List<MatchingRule>(), RulesStorageKey);
                PopulateRules(RulesStorageKey, rules, rulesGroupedByBucket);
            }
        }

        public bool RemoveRule(ICollection<MatchingRule> rules, ICollection<RulesGroupedByBucket> rulesGroupedByBucket, MatchingRule ruleToRemove)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            if (rulesGroupedByBucket == null)
            {
                throw new ArgumentNullException("rulesGroupedByBucket");
            }
            if (ruleToRemove == null)
            {
                throw new ArgumentNullException("ruleToRemove");
            }
            if (string.IsNullOrWhiteSpace(RulesStorageKey))
            {
                throw new InvalidOperationException("Unable to remove a matching rule at this time, the service has not yet loaded a matching rule set.");
            }

            RulesGroupedByBucket existingGroup = rulesGroupedByBucket.FirstOrDefault(g => g.Bucket == ruleToRemove.Bucket);
            if (existingGroup == null)
            {
                return false;
            }

            bool success1 = existingGroup.Rules.Remove(ruleToRemove);
            bool success2 = rules.Remove(ruleToRemove);
            MatchingRule removedRule = ruleToRemove;

            SaveRules(rules);

            this.logger.LogInfo(_ => "Matching Rule is being Removed: " + removedRule);
            if (!success1)
            {
                this.logger.LogWarning(_ => "Matching Rule was not removed successfully from the Grouped list: " + removedRule);
            }

            if (!success2)
            {
                this.logger.LogWarning(_ => "Matching Rule was not removed successfully from the flat list: " + removedRule);
            }

            return true;
        }

        public void SaveRules(IEnumerable<MatchingRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            this.ruleRepository.SaveRules(rules, RulesStorageKey);
        }

        protected virtual string BuildDefaultFileName()
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return Path.Combine(path, "MatchingRules.xml");
        }

        private static bool IsEqualButNotBlank(string operand1, string operand2)
        {
            if (string.IsNullOrWhiteSpace(operand1) || string.IsNullOrWhiteSpace(operand2))
            {
                return false;
            }

            return operand1 == operand2;
        }
    }
}