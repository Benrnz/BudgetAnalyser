using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.CommandWpf;
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
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly ITransactionRuleService ruleService;
        private bool doNotUseEditingRule;
        private bool doNotUseFlatListBoxVisibility;
        private bool doNotUseGroupByListBoxVisibility;
        private MatchingRule doNotUseSelectedRule;

        private bool doNotUseShown;

        private string doNotUseSortBy;
        private bool initialised;

        public RulesController([NotNull] IUiContext uiContext, [NotNull] ITransactionRuleService ruleService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (ruleService == null)
            {
                throw new ArgumentNullException("ruleService");
            }

            this.ruleService = ruleService;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            NewRuleController = uiContext.NewRuleController;

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

        public string AndOrText
        {
            get { return SelectedRule.And ? "AND" : "OR"; }
        }

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
                RaisePropertyChanged(() => AndOrText);
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
                string oldValue = this.doNotUseSortBy;
                this.doNotUseSortBy = value;
                RaisePropertyChanged(() => SortBy);
                GroupByListBoxVisibility = false;
                FlatListBoxVisibility = false;
                switch (SortBy)
                {
                    case BucketSortKey:
                        GroupByListBoxVisibility = true;
                        break;

                    case MatchesSortKey:
                    case DescriptionSortKey:
                        FlatListBoxVisibility = true;
                        break;

                    default:
                        this.doNotUseSortBy = oldValue;
                        throw new ArgumentException(value + " is not a valid sort by argument.");
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
            NewRuleController.AndChecked = true;
            NewRuleController.ShowDialog(Rules);

            NewRuleController.RuleCreated += OnNewRuleCreated;
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }
            this.initialised = true;
            Rules = new ObservableCollection<MatchingRule>();
            RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>();
            LoadRules();
        }

        public void SaveRules()
        {
            this.ruleService.SaveRules(Rules);
        }

        protected void LoadRules(string rulesFileName = "")
        {
            SortBy = BucketSortKey; // Defaults to Bucket sort order.
            if (string.IsNullOrWhiteSpace(rulesFileName))
            {
                this.ruleService.PopulateRules(Rules, RulesGroupedByBucket);
            }
            else
            {
                this.ruleService.PopulateRules(rulesFileName, Rules, RulesGroupedByBucket);
            }
        }

        private void AddToList(MatchingRule rule)
        {
            if (!this.ruleService.AddRule(Rules, RulesGroupedByBucket, rule))
            {
                return;
            }

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

            var rulesFileName = message.RehydratedModels[typeof(LastMatchingRulesLoadedV1)].AdaptModel<string>();
            LoadRules(rulesFileName);
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastRuleSet = new LastMatchingRulesLoadedV1
            {
                Model = this.ruleService.RulesStorageKey,
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

        private void OnNewRuleCreated(object sender, EventArgs eventArgs)
        {
            NewRuleController.RuleCreated -= OnNewRuleCreated;
            if (NewRuleController.NewRule != null)
            {
                AddToList(NewRuleController.NewRule);
            }
        }

        private void OnSaveRuleCommandExecute()
        {
            EditingRule = false;
            SaveRules();
            RaisePropertyChanged(() => AndOrText);
        }

        private void OnSortCommandExecute(string sortBy)
        {
            SortBy = sortBy;
        }

        private void RemoveRule()
        {
            if (EditingRule)
            {
                EditingRule = false;
            }

            var selectedRule = SelectedRule;
            if (!this.ruleService.RemoveRule(Rules, RulesGroupedByBucket, SelectedRule))
            {
                return;
            }

            EventHandler<MatchingRuleEventArgs> handler = RuleRemoved;
            if (handler != null)
            {
                handler(selectedRule, new MatchingRuleEventArgs { Rule = selectedRule });
            }

            SelectedRule = null;
        }
    }
}