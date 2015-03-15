using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class TransactionRuleService : ITransactionRuleService, ISupportsModelPersistence
    {
        private readonly ILogger logger;
        private readonly IMatchmaker matchmaker;
        private readonly IMatchingRuleFactory ruleFactory;
        private readonly IMatchingRuleRepository ruleRepository;
        private string rulesStorageKey;

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
            MatchingRules = new ObservableCollection<MatchingRule>();
            MatchingRulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>();
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;

        public ApplicationDataType DataType
        {
            get { return ApplicationDataType.MatchingRules; }
        }

        public int LoadSequence
        {
            get { return 50; }
        }

        public ObservableCollection<MatchingRule> MatchingRules { get; private set; }
        public ObservableCollection<RulesGroupedByBucket> MatchingRulesGroupedByBucket { get; private set; }

        public bool AddRule(MatchingRule ruleToAdd)
        {
            if (ruleToAdd == null)
            {
                throw new ArgumentNullException("ruleToAdd");
            }
            if (string.IsNullOrWhiteSpace(this.rulesStorageKey))
            {
                throw new InvalidOperationException("Unable to add a matching rule at this time, the service has not yet loaded a matching rule set.");
            }

            RulesGroupedByBucket existingGroup = MatchingRulesGroupedByBucket.FirstOrDefault(group => group.Bucket == ruleToAdd.Bucket);
            if (existingGroup == null)
            {
                var addNewGroup = new RulesGroupedByBucket(ruleToAdd.Bucket, new[] { ruleToAdd });
                MatchingRulesGroupedByBucket.Add(addNewGroup);
                MatchingRules.Add(ruleToAdd);
            }
            else
            {
                if (existingGroup.Rules.Contains(ruleToAdd))
                {
                    this.logger.LogWarning(l => "Attempt to add new rule failed. Rule already exists in Grouped collection. " + ruleToAdd);
                    return false;
                }
                existingGroup.Rules.Add(ruleToAdd);
                if (MatchingRules.Contains(ruleToAdd))
                {
                    this.logger.LogWarning(l => "Attempt to add new rule failed. Rule already exists in main collection. " + ruleToAdd);
                    return false;
                }

                MatchingRules.Add(ruleToAdd);
            }

            this.logger.LogInfo(_ => "Matching Rule Added: " + ruleToAdd);
            return true;
        }

        public void Close()
        {
            this.rulesStorageKey = null;
            MatchingRulesGroupedByBucket.Clear();
            MatchingRules.Clear();
            EventHandler handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
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

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            MatchingRules.Clear();
            MatchingRulesGroupedByBucket.Clear();
            this.rulesStorageKey = applicationDatabase.FullPath(applicationDatabase.MatchingRulesCollectionStorageKey);
            List<MatchingRule> repoRules;
            try
            {
                repoRules = (await this.ruleRepository.LoadRulesAsync(this.rulesStorageKey))
                    .OrderBy(r => r.Description)
                    .ToList();
            }
            catch (FileNotFoundException)
            {
                // If file not found occurs here, assume this is the first time the app has run, and create a new one.
                this.rulesStorageKey = BuildDefaultFileName();
                repoRules = new List<MatchingRule>();
            }

            foreach (MatchingRule rule in repoRules)
            {
                MatchingRules.Add(rule);
            }

            IEnumerable<RulesGroupedByBucket> grouped = repoRules.GroupBy(rule => rule.Bucket)
                .Where(group => group.Key != null)
                // this is to prevent showing rules that have a bucket code not currently in the current budget model. Happens when loading the demo or empty budget model.
                .Select(group => new RulesGroupedByBucket(group.Key, group))
                .OrderBy(group => group.Bucket.Code);

            foreach (RulesGroupedByBucket groupedByBucket in grouped)
            {
                MatchingRulesGroupedByBucket.Add(groupedByBucket);
            }

            EventHandler handler = NewDataSourceAvailable;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public bool Match(IEnumerable<Transaction> transactions)
        {
            return this.matchmaker.Match(transactions, MatchingRules);
        }

        public bool RemoveRule(MatchingRule ruleToRemove)
        {
            if (ruleToRemove == null)
            {
                throw new ArgumentNullException("ruleToRemove");
            }

            if (string.IsNullOrWhiteSpace(this.rulesStorageKey))
            {
                throw new InvalidOperationException("Unable to remove a matching rule at this time, the service has not yet loaded a matching rule set.");
            }

            RulesGroupedByBucket existingGroup = MatchingRulesGroupedByBucket.FirstOrDefault(g => g.Bucket == ruleToRemove.Bucket);
            if (existingGroup == null)
            {
                return false;
            }

            bool success1 = existingGroup.Rules.Remove(ruleToRemove);
            bool success2 = MatchingRules.Remove(ruleToRemove);
            MatchingRule removedRule = ruleToRemove;

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

        public async Task SaveAsync(IDictionary<ApplicationDataType, object> contextObjects)
        {
            EventHandler<AdditionalInformationRequestedEventArgs> handler = Saving;
            if (handler != null)
            {
                handler(this, new AdditionalInformationRequestedEventArgs());
            }

            var messages = new StringBuilder();
            if (ValidateModel(messages))
            {
                await this.ruleRepository.SaveRulesAsync(MatchingRules, this.rulesStorageKey);
            }
            else
            {
                throw new ValidationWarningException("Unable to save matching rules at this time, some data is invalid.\n" + messages);
            }

            EventHandler savedHandler = Saved;
            if (savedHandler != null)
            {
                savedHandler(this, EventArgs.Empty);
            }
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            if (handler != null)
            {
                handler(this, new ValidatingEventArgs());
            }
            return true;
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