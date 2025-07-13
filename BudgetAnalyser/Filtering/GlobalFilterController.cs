using System.Globalization;
using System.Text;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

// ReSharper disable ClassNeverInstantiated.Global
// Ctor and publics used by bindings.

namespace BudgetAnalyser.Filtering;

[AutoRegisterWithIoC(SingleInstance = true)]
public class GlobalFilterController : ControllerBase, IShellDialogToolTips
{
    private readonly IApplicationDatabaseService appDbService;
    private readonly IUserMessageBox userMessageBox;
    private BudgetModel? currentBudget;
    private Guid dialogCorrelationId;
    private string doNotUseAccountTypeSummary = string.Empty;
    private GlobalFilterCriteria doNotUseCriteria;
    private string doNotUseDateSummaryLine1 = string.Empty;
    private string doNotUseDateSummaryLine2 = string.Empty;

    public GlobalFilterController(IUiContext uiContext, IApplicationDatabaseService appDbService) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.appDbService = appDbService ?? throw new ArgumentNullException(nameof(appDbService));
        this.appDbService.NewDataSourceAvailable += OnNewFilter;
        this.userMessageBox = uiContext.UserPrompts.MessageBox;
        this.doNotUseCriteria = new GlobalFilterCriteria();
        this.currentBudget = uiContext.Controller<TabBudgetController>().CurrentBudget?.Model; //Likely always an empty budget before the bax file is loaded.

        Messenger.Register<GlobalFilterController, RequestFilterMessage>(this, static (r, m) => r.OnGlobalFilterRequested(m));
        Messenger.Register<GlobalFilterController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
        Messenger.Register<GlobalFilterController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        Messenger.Register<GlobalFilterController, RequestFilterChangeMessage>(this, static (r, m) => r.OnGlobalFilterChangeRequested(m));
        Messenger.Register<GlobalFilterController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
    }

    public string AccountTypeSummary
    {
        [UsedImplicitly]
        get => this.doNotUseAccountTypeSummary;
        private set
        {
            this.doNotUseAccountTypeSummary = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand AddPeriodCommand => new RelayCommand<DateOnly>(OnAddPeriodCommandExecute, d => d != DateOnly.MinValue);

    [UsedImplicitly]
    public ICommand BackPeriodCommand => new RelayCommand<DateOnly>(OnBackPeriodCommandExecute, d => d != DateOnly.MinValue);

    [UsedImplicitly]
    public ICommand ClearCommand => new RelayCommand(OnClearCommandExecute);

    public GlobalFilterCriteria Criteria
    {
        get => this.doNotUseCriteria;
        set
        {
            if (value == this.doNotUseCriteria)
            {
                return;
            }

            this.doNotUseCriteria = value;
            OnPropertyChanged();
            UpdateSummaries();
        }
    }

    public string DateSummaryLine1
    {
        [UsedImplicitly]
        get => this.doNotUseDateSummaryLine1;
        private set
        {
            this.doNotUseDateSummaryLine1 = value;
            OnPropertyChanged();
        }
    }

    public string DateSummaryLine2
    {
        [UsedImplicitly]
        get => this.doNotUseDateSummaryLine2;
        private set
        {
            this.doNotUseDateSummaryLine2 = value;
            OnPropertyChanged();
        }
    }

    public string ActionButtonToolTip => "Apply filter and close.";

    public string CloseButtonToolTip => "Cancel and do not change the filter.";

    public void PromptUserForDates()
    {
        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel) { CorrelationId = this.dialogCorrelationId, Title = "Global Date Filter" };
        Messenger.Send(dialogRequest);
    }

    private void OnAddPeriodCommandExecute(DateOnly date)
    {
        if (Criteria.BeginDate is not null && date == Criteria.BeginDate)
        {
            if (this.currentBudget!.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(14);
        }

        if (Criteria.EndDate is not null && date == Criteria.EndDate)
        {
            if (this.currentBudget!.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(1);
                return;
            }

            Criteria.EndDate = Criteria.EndDate.Value.AddDays(14);
        }
    }

    private void OnBackPeriodCommandExecute(DateOnly date)
    {
        if (Criteria.BeginDate is not null && date == Criteria.BeginDate)
        {
            if (this.currentBudget!.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(-1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(-14);
        }

        if (Criteria.EndDate is not null && date == Criteria.EndDate)
        {
            if (this.currentBudget!.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(-1);
                return;
            }

            Criteria.EndDate = Criteria.EndDate.Value.AddDays(-14);
        }
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        this.currentBudget = message.Budgets?.CurrentActiveBudget;
    }

    private void OnClearCommandExecute()
    {
        Criteria.BeginDate = null;
        Criteria.EndDate = null;
    }

    private void OnGlobalFilterChangeRequested(RequestFilterChangeMessage message)
    {
        Criteria = message.Criteria;
        SendFilterAppliedMessage();
    }

    private void OnGlobalFilterRequested(RequestFilterMessage message)
    {
        message.Criteria = Criteria;
    }

    private void OnNewFilter(object? sender, EventArgs e)
    {
        Criteria = this.appDbService.GlobalFilter;
        SendFilterAppliedMessage();
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        var validationMessages = new StringBuilder();
        if (!Criteria.Validate(validationMessages))
        {
            this.userMessageBox.Show(validationMessages.ToString(), "Invalid Filter Values!");
            return;
        }

        SendFilterAppliedMessage();
    }

    private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (message.Widget is DateFilterWidget)
        {
            PromptUserForDates();
        }
    }

    private void SendFilterAppliedMessage()
    {
        UpdateSummaries();
        Messenger.Send(new FilterAppliedMessage(this, Criteria));
    }

    private void UpdateSummaries()
    {
        DateSummaryLine1 = "No date filter applied.";
        DateSummaryLine2 = string.Empty;
        AccountTypeSummary = "No account filter applied.";

        if (Criteria.Cleared)
        {
            return;
        }

        if (Criteria.BeginDate is not null)
        {
            DateSummaryLine1 = string.Format(CultureInfo.CurrentCulture, "Filtered from: {0:d}", Criteria.BeginDate.Value);
        }

        if (Criteria.EndDate is not null)
        {
            DateSummaryLine2 = string.Format(CultureInfo.CurrentCulture, "up until: {0:d}", Criteria.EndDate.Value);
        }
    }
}
