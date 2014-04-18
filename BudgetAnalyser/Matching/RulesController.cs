using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class RulesController : ControllerBase, IInitializableController, IShowableController
    {
        public const string BucketSortKey = "Bucket";
        public const string DescriptionSortKey = "Description";
        public const string MatchesSortKey = "Matches";
        private readonly ILogger logger;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly IMatchingRuleRepository ruleRepository;
        private RulesGroupedByBucket addNewGroup;
        private MatchingRule addingNewRule;
        private bool doNotUseFlatListBoxVisibility;
        private bool doNotUseGroupByListBoxVisibility;
        private MatchingRule doNotUseSelectedRule;

        private bool doNotUseShown;

        private string doNotUseSortBy;

        private Guid debugId = Guid.NewGuid();

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
            this.logger.LogInfo(() => "RulesController Constructed with Id: " + this.debugId);
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
        public event EventHandler RuleAdded;

        /// <summary>
        ///     These events are required because the ListBoxes do not update when items are removed. God only knows why.
        /// </summary>
        public event EventHandler RuleRemoved;

        public event EventHandler SortChanged;

        public ICommand CloseCommand
        {
            get { return new RelayCommand(() => Shown = false); }
        }

        public ICommand DeleteRuleCommand
        {
            get { return new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand); }
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
        public ObservableCollection<RulesGroupedByBucket> RulesGroupedByBucket { get; set; }

        public MatchingRule SelectedRule
        {
            get { return this.doNotUseSelectedRule; }
            set
            {
                this.doNotUseSelectedRule = value;
                RaisePropertyChanged(() => SelectedRule);
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

        public void CreateNewRuleFromTransaction(Transaction transaction)
        {
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
                //Rules.AddingNew += OnAddingNewGroup;
                //Rules.AddNew();
                //Rules.AddingNew -= OnAddingNewGroup;
                //Rules.EndNew(0);
                //SaveRules();
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
                this.ruleRepository.SaveRules(new List<MatchingRule>(), GetFileName());
                LoadRules();
            }
        }

        public void SaveRules()
        {
            this.ruleRepository.SaveRules(Rules, GetFileName());
        }

        protected virtual string GetFileName()
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
            List<MatchingRule> rules = this.ruleRepository.LoadRules(GetFileName())
                .OrderBy(r => r.Description)
                .ToList();

            Rules = new ObservableCollection<MatchingRule>(rules);
            SortBy = BucketSortKey;

            IEnumerable<RulesGroupedByBucket> grouped = rules.GroupBy(rule => rule.Bucket)
                .Select(group => new RulesGroupedByBucket(group.Key, group))
                .OrderBy(group => group.Bucket.Code);

            RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>(grouped.ToList());
        }

        private void AddToList(MatchingRule rule)
        {
            RulesGroupedByBucket existingGroup = RulesGroupedByBucket.FirstOrDefault(group => group.Bucket == rule.Bucket);
            if (existingGroup == null)
            {
                this.addNewGroup = new RulesGroupedByBucket(rule.Bucket, new[] { rule });
                RulesGroupedByBucket.Add(this.addNewGroup);
                Rules.Add(rule);
                this.addNewGroup = null;
            }
            else
            {
                this.addingNewRule = rule;
                existingGroup.Rules.Add(this.addingNewRule);
                Rules.Add(rule);
                this.addingNewRule = null;
            }

            SaveRules();
            this.logger.LogInfo(() => "Matching Rule Added: " + rule);
            EventHandler handler = RuleAdded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private bool CanExecuteDeleteRuleCommand()
        {
            return SelectedRule != null;
        }

        private void OnAddingNewGroup(object sender, AddingNewEventArgs addingNewEventArgs)
        {
            addingNewEventArgs.NewObject = this.addNewGroup;
        }

        private void OnAddingNewRuleToGroup(object sender, AddingNewEventArgs addingNewEventArgs)
        {
            addingNewEventArgs.NewObject = this.addingNewRule;
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

        private void OnSortCommandExecute(string sortBy)
        {
            // TODO
            SortBy = sortBy;
        }

        private void RemoveRule()
        {
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

            EventHandler handler = RuleRemoved;
            if (handler != null)
            {
                handler(removedRule, EventArgs.Empty);
            }
        }
    }
}