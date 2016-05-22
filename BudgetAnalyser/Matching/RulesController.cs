using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

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
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly ITransactionRuleService ruleService;
        private bool doNotUseEditingRule;
        private bool doNotUseFlatListBoxVisibility;
        private bool doNotUseGroupByListBoxVisibility;
        private MatchingRule doNotUseSelectedRule;
        private bool doNotUseShown;
        private string doNotUseSortBy;
        private bool initialised;

        public RulesController([NotNull] IUiContext uiContext, [NotNull] ITransactionRuleService ruleService, [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (ruleService == null)
            {
                throw new ArgumentNullException(nameof(ruleService));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.ruleService = ruleService;
            this.applicationDatabaseService = applicationDatabaseService;

            this.questionBox = uiContext.UserPrompts.YesNoBox;
            NewRuleController = uiContext.NewRuleController;
            MessengerInstance = uiContext.Messenger;

            this.ruleService.Closed += OnClosedNotificationReceived;
            this.ruleService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
            this.ruleService.Saved += OnSavedNotificationReceived;
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
        public string AndOrText => SelectedRule == null ? null : SelectedRule.And ? "AND" : "OR";

        [UsedImplicitly]
        public ICommand CloseCommand => new RelayCommand(() => Shown = false);

        [UsedImplicitly]
        public ICommand DeleteRuleCommand => new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand);

        public bool EditingRule
        {
            get { return this.doNotUseEditingRule; }
            private set
            {
                this.doNotUseEditingRule = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => ShowReadOnlyRuleDetails);
            }
        }

        [UsedImplicitly]
        public ICommand EditRuleCommand => new RelayCommand(OnEditRuleCommandExecute, () => SelectedRule != null);

        public bool FlatListBoxVisibility
        {
            get { return this.doNotUseFlatListBoxVisibility; }
            set
            {
                this.doNotUseFlatListBoxVisibility = value;
                RaisePropertyChanged();
            }
        }

        public bool GroupByListBoxVisibility
        {
            get { return this.doNotUseGroupByListBoxVisibility; }
            set
            {
                this.doNotUseGroupByListBoxVisibility = value;
                RaisePropertyChanged();
            }
        }

        public NewRuleController NewRuleController { get; }
        public ObservableCollection<MatchingRule> Rules { get; private set; }
        public ObservableCollection<RulesGroupedByBucket> RulesGroupedByBucket { get; private set; }

        public MatchingRule SelectedRule
        {
            get { return this.doNotUseSelectedRule; }
            set
            {
                this.doNotUseSelectedRule = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => ShowReadOnlyRuleDetails);
                RaisePropertyChanged(() => AndOrText);
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
                RaisePropertyChanged();
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

        public string SortBy
        {
            get { return this.doNotUseSortBy; }
            set
            {
                string oldValue = this.doNotUseSortBy;
                this.doNotUseSortBy = value;
                RaisePropertyChanged();
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
                handler?.Invoke(this, EventArgs.Empty);
            }
        }

        [UsedImplicitly]
        public ICommand SortCommand => new RelayCommand<string>(OnSortCommandExecute);

        public void CreateNewRuleFromTransaction([NotNull] Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (string.IsNullOrWhiteSpace(transaction.BudgetBucket?.Code))
            {
                MessageBox.Show("Select a Bucket code first.");
                return;
            }

            NewRuleController.Initialize();
            NewRuleController.Bucket = transaction.BudgetBucket;
            NewRuleController.Description.Value = transaction.Description;
            NewRuleController.Reference1.Value = transaction.Reference1;
            NewRuleController.Reference2.Value = transaction.Reference2;
            NewRuleController.Reference3.Value = transaction.Reference3;
            NewRuleController.TransactionType.Value = transaction.TransactionType.Name;
            NewRuleController.Amount.Value = transaction.Amount;
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
        }

        private void AddToList(MatchingRule rule)
        {
            if (!(rule is SingleUseMatchingRule))
            {
                Rules.Add(rule);
                RulesGroupedByBucket.Single(g => g.Bucket == rule.Bucket).Rules.Add(rule);
                EventHandler<MatchingRuleEventArgs> handler = RuleAdded;
                handler?.Invoke(this, new MatchingRuleEventArgs { Rule = rule });
            }

            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
        }

        private bool CanExecuteDeleteRuleCommand()
        {
            return SelectedRule != null;
        }

        private void OnClosedNotificationReceived(object sender, EventArgs eventArgs)
        {
            Rules.Clear();
            RulesGroupedByBucket.Clear();
            SelectedRule = null;
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
            if (EditingRule)
            {
                // Finished editing, re-lock the editing form.
                EditingRule = false;
            }
            else
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
                EditingRule = true;
            }
        }

        private void OnNewDataSourceAvailableNotificationReceived(object sender, EventArgs e)
        {
            SortBy = BucketSortKey; // Defaults to Bucket sort order.
            Rules = new ObservableCollection<MatchingRule>(this.ruleService.MatchingRules);
            RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>(this.ruleService.MatchingRulesGroupedByBucket);
            SelectedRule = null;
            RaisePropertyChanged(() => Rules);
            RaisePropertyChanged(() => RulesGroupedByBucket);
        }

        private void OnNewRuleCreated(object sender, EventArgs eventArgs)
        {
            NewRuleController.RuleCreated -= OnNewRuleCreated;
            if (NewRuleController.NewRule != null)
            {
                AddToList(NewRuleController.NewRule);
            }
        }

        private void OnSavedNotificationReceived(object sender, EventArgs eventArgs)
        {
            EditingRule = false;
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

            MatchingRule selectedRule = SelectedRule;
            if (!this.ruleService.RemoveRule(SelectedRule))
            {
                return;
            }

            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);

            EventHandler<MatchingRuleEventArgs> handler = RuleRemoved;
            handler?.Invoke(selectedRule, new MatchingRuleEventArgs { Rule = selectedRule });

            SelectedRule = null;
        }
    }
}