using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class TransactionRuleService : ITransactionRuleService
    {
        private readonly ILogger logger;
        private readonly IMatchmaker matchMaker;
        private readonly IMatchingRuleRepository ruleRepository;

        public TransactionRuleService([NotNull] IMatchingRuleRepository ruleRepository, [NotNull] ILogger logger, [NotNull] IMatchmaker matchMaker)
        {
            if (ruleRepository == null)
            {
                throw new ArgumentNullException("ruleRepository");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (matchMaker == null)
            {
                throw new ArgumentNullException("matchMaker");
            }

            this.ruleRepository = ruleRepository;
            this.logger = logger;
            this.matchMaker = matchMaker;
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

        public bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules)
        {
            return this.matchMaker.Match(transactions, rules);
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
    }
}