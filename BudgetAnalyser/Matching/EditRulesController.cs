using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Matching;

/// <summary>
///     The Controller for <see cref="EditRulesUserControl" /> .
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class EditRulesController : ControllerBase
{
    public const string BucketSortKey = "Bucket";
    public const string DescriptionSortKey = "Description";
    public const string MatchesSortKey = "Matches";
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly Guid dialogCorrelationId = Guid.NewGuid();
    private readonly IUserQuestionBoxYesNo questionBox;
    private readonly ITransactionRuleService ruleService;

    public EditRulesController(
        IMessenger messenger,
        UserPrompts userPrompts,
        ITransactionRuleService ruleService,
        IApplicationDatabaseFacade applicationDatabaseService)
        : base(messenger)
    {
        this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));

        this.questionBox = userPrompts.YesNoBox ?? throw new ArgumentNullException(nameof(userPrompts.YesNoBox));
        DeleteRuleCommand = new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand);
        SelectRuleCommand = new RelayCommand(OnEditRuleCommandExecute, () => SelectedRule is not null);
        SortCommand = new RelayCommand<string?>(OnSortCommandExecute);
        ShowRulesCommand = new RelayCommand(ShowDialog);

        this.ruleService.Closed += OnClosedNotificationReceived;
        this.ruleService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        this.ruleService.Saved += OnSavedNotificationReceived;

        Rules = new ObservableCollection<MatchingRule>();
        RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>();

        Messenger.Register<EditRulesController, RuleCreatedMessage>(this, OnRuleCreated);
    }

    public string? AndOrText => SelectedRule is null ? null : SelectedRule.And ? "AND" : "OR";

    public IRelayCommand DeleteRuleCommand { get; }

    public bool EditingRule
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowReadOnlyRuleDetails));
        }
    }

    public bool FlatListBoxVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool GroupByListBoxVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MatchingRule> Rules { get; private set; }
    public ObservableCollection<RulesGroupedByBucket> RulesGroupedByBucket { get; private set; }

    public MatchingRule? SelectedRule
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowReadOnlyRuleDetails));
            OnPropertyChanged(nameof(AndOrText));
            DeleteRuleCommand.NotifyCanExecuteChanged();
            SelectRuleCommand.NotifyCanExecuteChanged();
        }
    }

    public IRelayCommand SelectRuleCommand { get; }

    public bool ShowReadOnlyRuleDetails
    {
        get
        {
            var result = SelectedRule is not null && !EditingRule;
            return result;
        }
    }

    public ICommand ShowRulesCommand { get; }

    public string? SortBy
    {
        get;
        set
        {
            var oldValue = field;
            field = value;
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
                    field = oldValue;
                    throw new ArgumentException(value + " is not a valid sort by argument.");
            }

            Messenger.Send<RuleSortChangedMessage>();
        }
    }

    public ICommand SortCommand { get; }


    /// <summary>
    ///     Registers the view's message handlers with the messenger so the view is notified of rule and sort changes.
    ///     All messenger interaction is managed here, keeping the view free of messenger dependencies.
    /// </summary>
    public void RegisterViewHandlers(EditRulesUserControl view)
    {
        Messenger.Register<EditRulesUserControl, RuleAddedMessage>(view, static (r, m) => r.OnRuleAdded(m.Rule));
        Messenger.Register<EditRulesUserControl, RuleRemovedMessage>(view, static (r, m) => r.OnRuleRemoved(m.Rule));
        Messenger.Register<EditRulesUserControl, RuleSortChangedMessage>(view, static (r, _) => r.OnSortChanged());
    }

    public void ShowDialog()
    {
        if (Rules.Count == 0)
        {
            Reset();
        }

        Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.Close)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Edit Matching Rules"
        });
    }

    /// <summary>
    ///     Unregisters all message handlers previously registered for the given view.
    /// </summary>
    public void UnregisterViewHandlers(EditRulesUserControl view)
    {
        Messenger.UnregisterAll(view);
    }

    private bool CanExecuteDeleteRuleCommand()
    {
        return SelectedRule is not null;
    }

    private void OnClosedNotificationReceived(object? sender, EventArgs eventArgs)
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

    private void OnNewDataSourceAvailableNotificationReceived(object? sender, EventArgs e)
    {
        Reset();
    }

    private void OnRuleCreated(object recipient, RuleCreatedMessage message)
    {
        // Add to list
        if (!(message.Rule is SingleUseMatchingRule))
        {
            Rules.Add(message.Rule);
            RulesGroupedByBucket.Single(g => g.Bucket == message.Rule.Bucket).Rules.Add(message.Rule);
            Messenger.Send(new RuleAddedMessage(message.Rule));
        }

        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
    }


    private void OnSavedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        EditingRule = false;
        OnPropertyChanged(nameof(AndOrText));
    }

    private void OnSortCommandExecute(string? sortBy)
    {
        SortBy = sortBy;
    }

    private void RemoveRule()
    {
        if (EditingRule)
        {
            EditingRule = false;
        }

        var selectedRule = SelectedRule ?? throw new InvalidOperationException("Selected Rule is null, and not selected.");
        if (!this.ruleService.RemoveRule(SelectedRule))
        {
            return;
        }

        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);

        Messenger.Send(new RuleRemovedMessage(selectedRule));

        SelectedRule = null;
    }

    private void Reset()
    {
        SortBy = BucketSortKey; // Defaults to Bucket sort order.
        Rules = new ObservableCollection<MatchingRule>(this.ruleService.MatchingRules);
        RulesGroupedByBucket = new ObservableCollection<RulesGroupedByBucket>(this.ruleService.MatchingRulesGroupedByBucket);
        SelectedRule = null;
        OnPropertyChanged(nameof(Rules));
        OnPropertyChanged(nameof(RulesGroupedByBucket));
    }
}
