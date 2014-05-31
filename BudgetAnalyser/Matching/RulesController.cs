using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Matching
{
    /// <summary>
    ///     The Controller for <see cref="EditRulesUserControl" /> .
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class RulesController : ControllerBase, IInitializableController, IShowableController
    {
        public const string BucketSortKey = "Bucket";
        public const string DescriptionSortKey = "Description";
        public const string MatchesSortKey = "Matches";
        private readonly ILogger logger;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly IMatchingRuleRepository ruleRepository;
        private bool doNotUseEditingRule;
        private bool doNotUseFlatListBoxVisibility;
        private bool doNotUseGroupByListBoxVisibility;
        private MatchingRule doNotUseSelectedRule;

        private bool doNotUseShown;

        private string doNotUseSortBy;

        /// <summary>
        ///     Only used if a custom matching rules file is being used. If this is null when the application state has loaded
        ///     message
        ///     arrives nothing will happen, if its a string value this will be used as a full file and path.
        /// </summary>
        private string rulesFileName;

        public RulesController(
            [NotNull] UiContext uiContext,
            [NotNull] IMatchingRuleRepository ruleRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            this.logger = uiContext.Logger;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.ruleRepository = ruleRepository;
            NewRuleController = uiContext.NewRuleController;
            RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>();

            MessengerInstance = uiContext.Messenger;
            uiContext.Messenger.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            uiContext.Messenger.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        /// <summary>
        ///     These events are required because the ListBoxes do not update when items are added. God only knows why.
        /// </summary>
        public event EventHandler<MatchingRuleEventArgs> RuleAdded;

        /// <summary>
        ///     These events are required because the ListBoxes do not update when items are removed. God only knows why.
        /// </summary>
        public event EventHandler<MatchingRuleEventArgs> RuleRemoved;

        public event EventHandler SortChanged;

        public ICommand CloseCommand
        {
            get { return new RelayCommand(() => Shown = false); }
        }

        public ICommand DeleteRuleCommand
        {
            get { return new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand); }
        }

        public ICommand EditRuleCommand
        {
            get { return new RelayCommand(OnEditRuleCommandExecute, () => SelectedRule != null); }
        }

        public bool EditingRule
        {
            get { return this.doNotUseEditingRule; }
            private set
            {
                this.doNotUseEditingRule = value;
                RaisePropertyChanged(() => EditingRule);
                RaisePropertyChanged(() => ShowReadOnlyRuleDetails);
            }
        }

        public bool FlatListBoxVisibility
        {
            get { return this.doNotUseFlatListBoxVisibility; }
            set
            {
                this.doNotUseFlatListBoxVisibility = value;
                RaisePropertyChanged(() => FlatListBoxVisibility);
            }
        }

        public bool GroupByListBoxVisibility
        {
            get { return this.doNotUseGroupByListBoxVisibility; }
            set
            {
                this.doNotUseGroupByListBoxVisibility = value;
                RaisePropertyChanged(() => GroupByListBoxVisibility);
            }
        }

        public NewRuleController NewRuleController { get; private set; }

        public ObservableCollection<MatchingRule> Rules { get; private set; }
        public ObservableCollection<RulesGroupedByBucket> RulesGroupedByBucket { get; private set; }

        public ICommand SaveRuleCommand
        {
            get { return new RelayCommand(OnSaveRuleCommandExecute, () => SelectedRule != null && EditingRule); }
        }

        public MatchingRule SelectedRule
        {
            get { return this.doNotUseSelectedRule; }
            set
            {
                this.doNotUseSelectedRule = value;
                RaisePropertyChanged(() => SelectedRule);
                RaisePropertyChanged(() => ShowReadOnlyRuleDetails);
            }
        }

        public bool ShowReadOnlyRuleDetails
        {
            get
            {
                bool result = SelectedRule != null && !EditingRule;
                return result;
            }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown)
                {
                    return;
                }
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public string SortBy
        {
            get { return this.doNotUseSortBy; }
            set
            {
                this.doNotUseSortBy = value;
                RaisePropertyChanged(() => SortBy);
                GroupByListBoxVisibility = false;
                FlatListBoxVisibility = false;
                switch (this.doNotUseSortBy)
                {
                    case BucketSortKey:
                        GroupByListBoxVisibility = true;
                        break;

                    case MatchesSortKey:
                    case DescriptionSortKey:
                        FlatListBoxVisibility = true;
                        break;

                    default:
                        throw new ArgumentException(this.doNotUseSortBy + " is not a valid sort by argument.");
                }

                EventHandler handler = SortChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        public ICommand SortCommand
        {
            get { return new RelayCommand<string>(OnSortCommandExecute); }
        }

        public void CreateNewRuleFromTransaction([NotNull] Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction.BudgetBucket == null || string.IsNullOrWhiteSpace(transaction.BudgetBucket.Code))
            {
                MessageBox.Show("Select a Bucket code first.");
                return;
            }

            NewRuleController.Initialize();
            NewRuleController.Bucket = transaction.BudgetBucket;
            NewRuleController.Description = transaction.Description;
            NewRuleController.Reference1 = transaction.Reference1;
            NewRuleController.Reference2 = transaction.Reference2;
            NewRuleController.Reference3 = transaction.Reference3;
            NewRuleController.TransactionType = transaction.TransactionType.Name;
            NewRuleController.Amount = transaction.Amount;
            NewRuleController.ShowDialog(Rules);

            if (NewRuleController.NewRule != null)
            {
                AddToList(NewRuleController.NewRule);
            }
        }

        public void Initialize()
        {
            try
            {
                LoadRules();
            }
            catch (FileNotFoundException)
            {
                // If file not found occurs here, assume this is the first time the app has run, and create a new one.
                this.ruleRepository.SaveRules(new List<MatchingRule>(), BuildDefaultFileName());
                LoadRules();
            }
        }

        public void SaveRules()
        {
            this.ruleRepository.SaveRules(Rules, BuildDefaultFileName());
        }

        protected virtual string BuildDefaultFileName()
        {
            if (string.IsNullOrWhiteSpace(this.rulesFileName))
            {
                string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                return Path.Combine(path, "MatchingRules.xml");
            }

            return this.rulesFileName;
        }

        protected void LoadRules()
        {
            List<MatchingRule> rules = this.ruleRepository.LoadRules(BuildDefaultFileName())
                .OrderBy(r => r.Description)
                .ToList();

            Rules = new ObservableCollection<MatchingRule>(rules);
            SortBy = BucketSortKey;

            IEnumerable<RulesGroupedByBucket> grouped = rules.GroupBy(rule => rule.Bucket)
                .Where(group => group.Key != null)
                // this is to prevent showing rules that have a bucket code not currently in the current budget model. Happens when loading the demo or empty budget model.
                .Select(group => new RulesGroupedByBucket(group.Key, group))
                .OrderBy(group => group.Bucket.Code);

            RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>(grouped.ToList());
        }

        private void AddToList(MatchingRule rule)
        {
            RulesGroupedByBucket existingGroup = RulesGroupedByBucket.FirstOrDefault(group => group.Bucket == rule.Bucket);
            if (existingGroup == null)
            {
                var addNewGroup = new RulesGroupedByBucket(rule.Bucket, new[] { rule });
                RulesGroupedByBucket.Add(addNewGroup);
                Rules.Add(rule);
            }
            else
            {
                existingGroup.Rules.Add(rule);
                Rules.Add(rule);
            }

            SaveRules();
            this.logger.LogInfo(() => "Matching Rule Added: " + rule);
            EventHandler<MatchingRuleEventArgs> handler = RuleAdded;
            if (handler != null)
            {
                handler(this, new MatchingRuleEventArgs { Rule = rule });
            }
        }

        private bool CanExecuteDeleteRuleCommand()
        {
            return SelectedRule != null;
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(LastMatchingRulesLoadedV1)))
            {
                return;
            }

            this.rulesFileName = message.RehydratedModels[typeof(LastMatchingRulesLoadedV1)].AdaptModel<string>();
            LoadRules();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastRuleSet = new LastMatchingRulesLoadedV1
            {
                Model = this.rulesFileName,
            };
            message.PersistThisModel(lastRuleSet);
        }

        private void OnDeleteRuleCommandExecute()
        {
            if (SelectedRule == null)
            {
                return;
            }

            bool? certainty = this.questionBox.Show("Delete this rule?", "Are you sure?");
            if (certainty != null && certainty.Value)
            {
                RemoveRule();
            }
        }

        private void OnEditRuleCommandExecute()
        {
            EditingRule = true;
        }

        private void OnSaveRuleCommandExecute()
        {
            EditingRule = false;
            SaveRules();
        }

        private void OnSortCommandExecute(string sortBy)
        {
            // TODO
            SortBy = sortBy;
        }

        private void RemoveRule()
        {
            if (EditingRule)
            {
                EditingRule = false;
            }

            RulesGroupedByBucket existingGroup = RulesGroupedByBucket.FirstOrDefault(g => g.Bucket == SelectedRule.Bucket);
            if (existingGroup == null)
            {
                return;
            }

            bool success1 = existingGroup.Rules.Remove(SelectedRule);
            bool success2 = Rules.Remove(SelectedRule);
            MatchingRule removedRule = SelectedRule;
            SelectedRule = null;
            SaveRules();

            this.logger.LogInfo(() => "Matching Rule is being Removed: " + removedRule);
            if (!success1)
            {
                this.logger.LogWarning(() => "Matching Rule was not removed successfully from the Grouped list: " + removedRule);
            }

            if (!success2)
            {
                this.logger.LogWarning(() => "Matching Rule was not removed successfully from the flat list: " + removedRule);
            }

            EventHandler<MatchingRuleEventArgs> handler = RuleRemoved;
            if (handler != null)
            {
                handler(removedRule, new MatchingRuleEventArgs { Rule = removedRule });
            }
        }
    }
}