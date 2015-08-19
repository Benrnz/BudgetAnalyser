using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
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
                throw new ArgumentNullException(nameof(ruleRepository));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (matchmaker == null)
            {
                throw new ArgumentNullException(nameof(matchmaker));
            }

            if (ruleFactory == null)
            {
                throw new ArgumentNullException(nameof(ruleFactory));
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
        public ApplicationDataType DataType => ApplicationDataType.MatchingRules;
        public int LoadSequence => 50;
        public ObservableCollection<MatchingRule> MatchingRules { get; }
        public ObservableCollection<RulesGroupedByBucket> MatchingRulesGroupedByBucket { get; }

        public void Close()
        {
            this.rulesStorageKey = null;
            MatchingRulesGroupedByBucket.Clear();
            MatchingRules.Clear();
            EventHandler handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.MatchingRulesCollectionStorageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            await this.ruleRepository.CreateNewAndSaveAsync(applicationDatabase.MatchingRulesCollectionStorageKey);
            await LoadAsync(applicationDatabase);
        }

        public MatchingRule CreateNewRule(string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
        {
            MatchingRule rule = this.ruleFactory.CreateNewRule(bucketCode, description, references, transactionTypeName, amount, andMatching);
            AddRule(rule);
            return rule;
        }

        public SingleUseMatchingRule CreateNewSingleUseRule(string bucketCode, string description, string[] references, string transactionTypeName, decimal? amount, bool andMatching)
        {
            SingleUseMatchingRule rule = this.ruleFactory.CreateNewSingleUseRule(bucketCode, description, references, transactionTypeName, amount, andMatching);
            AddRule(rule);
            return rule;
        }

        public bool IsRuleSimilar(MatchingRule rule, decimal amount, string description, string[] references, string transactionTypeName)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (references == null)
            {
                throw new ArgumentNullException(nameof(references));
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
                repoRules = (await this.ruleRepository.LoadAsync(this.rulesStorageKey))
                    .OrderBy(r => r.Description)
                    .ToList();
            }
            catch (FileNotFoundException)
            {
                // If file not found occurs here, assume this is the first time the app has run, and create a new one.
                this.rulesStorageKey = BuildDefaultFileName();
                repoRules = this.ruleRepository.CreateNew().ToList();
            }

            InitialiseTheRulesCollections(repoRules);

            EventHandler handler = NewDataSourceAvailable;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public bool Match(IEnumerable<Transaction> transactions)
        {
            bool matchesMade = this.matchmaker.Match(transactions, MatchingRules);
            foreach (SingleUseMatchingRule rule in MatchingRules.OfType<SingleUseMatchingRule>().ToList())
            {
                if (rule.MatchCount > 0)
                {
                    RemoveRule(rule);
                }
            }
            return matchesMade;
        }

        public bool RemoveRule(MatchingRule ruleToRemove)
        {
            if (ruleToRemove == null)
            {
                throw new ArgumentNullException(nameof(ruleToRemove));
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

        public async Task SaveAsync(IReadOnlyDictionary<ApplicationDataType, object> contextObjects)
        {
            EventHandler<AdditionalInformationRequestedEventArgs> handler = Saving;
            handler?.Invoke(this, new AdditionalInformationRequestedEventArgs());

            var messages = new StringBuilder();
            if (ValidateModel(messages))
            {
                await this.ruleRepository.SaveAsync(MatchingRules, this.rulesStorageKey);
            }
            else
            {
                throw new ValidationWarningException("Unable to save matching rules at this time, some data is invalid.\n" + messages);
            }

            EventHandler savedHandler = Saved;
            savedHandler?.Invoke(this, EventArgs.Empty);
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            handler?.Invoke(this, new ValidatingEventArgs());
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

        private void AddRule(MatchingRule ruleToAdd)
        {
            if (ruleToAdd == null)
            {
                throw new ArgumentNullException(nameof(ruleToAdd));
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
                    return;
                }
                existingGroup.Rules.Add(ruleToAdd);
                if (MatchingRules.Contains(ruleToAdd))
                {
                    this.logger.LogWarning(l => "Attempt to add new rule failed. Rule already exists in main collection. " + ruleToAdd);
                    return;
                }

                MatchingRules.Add(ruleToAdd);
            }

            this.logger.LogInfo(_ => "Matching Rule Added: " + ruleToAdd);
        }

        private void InitialiseTheRulesCollections(List<MatchingRule> repoRules)
        {
            foreach (MatchingRule rule in repoRules)
            {
                MatchingRules.Add(rule);
            }

            IEnumerable<RulesGroupedByBucket> grouped = repoRules.GroupBy(rule => rule.Bucket)
                .Where(group => @group.Key != null)
                // this is to prevent showing rules that have a bucket code not currently in the current budget model. Happens when loading the demo or empty budget model.
                .Select(group => new RulesGroupedByBucket(@group.Key, @group))
                .OrderBy(group => @group.Bucket.Code);

            foreach (RulesGroupedByBucket groupedByBucket in grouped)
            {
                MatchingRulesGroupedByBucket.Add(groupedByBucket);
            }
        }
    }
}