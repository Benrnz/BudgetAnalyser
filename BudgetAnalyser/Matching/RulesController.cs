using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Matching;

/// <summary>
///     The Controller for <see cref="EditRulesUserControl" /> .
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class RulesController : ControllerBase, IInitializableController, IShowableController
{
    public const string BucketSortKey = "Bucket";
    public const string DescriptionSortKey = "Description";
    public const string MatchesSortKey = "Matches";
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly IUserQuestionBoxYesNo questionBox;
    private readonly ITransactionRuleService ruleService;
    private bool doNotUseEditingRule;
    private bool doNotUseFlatListBoxVisibility;
    private bool doNotUseGroupByListBoxVisibility;
    private MatchingRule doNotUseSelectedRule;
    private bool doNotUseShown;
    private string doNotUseSortBy;
    private bool initialised;

    public RulesController(IUiContext uiContext, ITransactionRuleService ruleService, IApplicationDatabaseFacade applicationDatabaseService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));

        this.questionBox = uiContext.UserPrompts.YesNoBox;
        NewRuleController = uiContext.NewRuleController;

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
    public string AndOrText => SelectedRule is null ? null : SelectedRule.And ? "AND" : "OR";

    [UsedImplicitly]
    public ICommand CloseCommand => new RelayCommand(() => Shown = false);

    [UsedImplicitly]
    public ICommand DeleteRuleCommand => new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand);

    public bool EditingRule
    {
        get => this.doNotUseEditingRule;
        private set
        {
            this.doNotUseEditingRule = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowReadOnlyRuleDetails));
        }
    }

    [UsedImplicitly]
    public ICommand EditRuleCommand => new RelayCommand(OnEditRuleCommandExecute, () => SelectedRule is not null);

    public bool FlatListBoxVisibility
    {
        get => this.doNotUseFlatListBoxVisibility;
        set
        {
            this.doNotUseFlatListBoxVisibility = value;
            OnPropertyChanged();
        }
    }

    public bool GroupByListBoxVisibility
    {
        get => this.doNotUseGroupByListBoxVisibility;
        set
        {
            this.doNotUseGroupByListBoxVisibility = value;
            OnPropertyChanged();
        }
    }

    public NewRuleController NewRuleController { get; }
    public ObservableCollection<MatchingRule> Rules { get; private set; }
    public ObservableCollection<RulesGroupedByBucket> RulesGroupedByBucket { get; private set; }

    public MatchingRule SelectedRule
    {
        get => this.doNotUseSelectedRule;
        set
        {
            this.doNotUseSelectedRule = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowReadOnlyRuleDetails));
            OnPropertyChanged(nameof(AndOrText));
        }
    }

    public bool ShowReadOnlyRuleDetails
    {
        get
        {
            var result = SelectedRule is not null && !EditingRule;
            return result;
        }
    }

    public string SortBy
    {
        get => this.doNotUseSortBy;
        set
        {
            var oldValue = this.doNotUseSortBy;
            this.doNotUseSortBy = value;
            OnPropertyChanged();
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

            var handler = SortChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    [UsedImplicitly]
    public ICommand SortCommand => new RelayCommand<string>(OnSortCommandExecute);

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

    public bool Shown
    {
        get => this.doNotUseShown;
        set
        {
            if (value == this.doNotUseShown)
            {
                return;
            }

            this.doNotUseShown = value;
            OnPropertyChanged();
        }
    }

    public void CreateNewRuleFromTransaction(Transaction transaction)
    {
        if (transaction is null)
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

    private void AddToList(MatchingRule rule)
    {
        if (!(rule is SingleUseMatchingRule))
        {
            Rules.Add(rule);
            RulesGroupedByBucket.Single(g => g.Bucket == rule.Bucket).Rules.Add(rule);
            var handler = RuleAdded;
            handler?.Invoke(this, new MatchingRuleEventArgs { Rule = rule });
        }

        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
    }

    private bool CanExecuteDeleteRuleCommand()
    {
        return SelectedRule is not null;
    }

    private void OnClosedNotificationReceived(object sender, EventArgs eventArgs)
    {
        Rules.Clear();
        RulesGroupedByBucket.Clear();
        SelectedRule = null;
    }

    private void OnDeleteRuleCommandExecute()
    {
        if (SelectedRule is null)
        {
            return;
        }

        var certainty = this.questionBox.Show("Delete this rule?", "Are you sure?");
        if (certainty is not null && certainty.Value)
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
        OnPropertyChanged(nameof(Rules));
        OnPropertyChanged(nameof(RulesGroupedByBucket));
    }

    private void OnNewRuleCreated(object sender, EventArgs eventArgs)
    {
        NewRuleController.RuleCreated -= OnNewRuleCreated;
        if (NewRuleController.NewRule is not null)
        {
            AddToList(NewRuleController.NewRule);
        }
    }

    private void OnSavedNotificationReceived(object sender, EventArgs eventArgs)
    {
        EditingRule = false;
        OnPropertyChanged(nameof(AndOrText));
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
        if (!this.ruleService.RemoveRule(SelectedRule))
        {
            return;
        }

        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);

        var handler = RuleRemoved;
        handler?.Invoke(selectedRule, new MatchingRuleEventArgs { Rule = selectedRule });

        SelectedRule = null;
    }
}
