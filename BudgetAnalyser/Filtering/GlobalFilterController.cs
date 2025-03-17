using System.Globalization;
using System.Text;
using System.Windows.Input;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
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
    private readonly IUserMessageBox userMessageBox;
    private BudgetModel currentBudget;
    private Guid dialogCorrelationId;
    private string doNotUseAccountTypeSummary;
    private GlobalFilterCriteria doNotUseCriteria;
    private string doNotUseDateSummaryLine1;
    private string doNotUseDateSummaryLine2;

    public GlobalFilterController([NotNull] UiContext uiContext) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.userMessageBox = uiContext.UserPrompts.MessageBox;
        this.doNotUseCriteria = new GlobalFilterCriteria();
        this.currentBudget = uiContext.BudgetController?.CurrentBudget?.Model; //Likely always an empty budget before the bax file is loaded.

        // Messenger.Register<BudgetController, ShellDialogResponseMessage>(this, static (r, m) => r.OnPopUpResponseReceived(m));
        Messenger.Register<GlobalFilterController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m));
        Messenger.Register<GlobalFilterController, ApplicationStateLoadFinishedMessage>(this, static (r, m) => r.OnApplicationStateLoadFinished(m));
        Messenger.Register<GlobalFilterController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));
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
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(14);
        }

        if (Criteria.EndDate is not null && date == Criteria.EndDate)
        {
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(1);
                return;
            }

            Criteria.EndDate = Criteria.EndDate.Value.AddDays(14);
        }
    }

    private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
    {
        var filterState = message.ElementOfType<PersistentFiltersApplicationState>();
        if (filterState is null)
        {
            return;
        }

        Criteria = new GlobalFilterCriteria
        {
            BeginDate = filterState.BeginDate is null ? null : DateOnly.FromDateTime(filterState.BeginDate.Value),
            EndDate = filterState.EndDate is null ? null : DateOnly.FromDateTime(filterState.EndDate.Value)
        };

        SendFilterAppliedMessage();
    }

    private void OnApplicationStateLoadFinished(ApplicationStateLoadFinishedMessage message)
    {
        if (Criteria is null || Criteria.Cleared)
        {
            SendFilterAppliedMessage();
        }
    }

    private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
    {
        var noCriteria = Criteria is null;
        var filterState = new PersistentFiltersApplicationState
        {
            BeginDate = noCriteria ? null : Criteria!.BeginDate?.ToDateTime(TimeOnly.MinValue),
            EndDate = noCriteria ? null : Criteria!.EndDate?.ToDateTime(TimeOnly.MinValue)
        };

        message.PersistThisModel(filterState);
    }

    private void OnBackPeriodCommandExecute(DateOnly date)
    {
        if (Criteria.BeginDate is not null && date == Criteria.BeginDate)
        {
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(-1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(-14);
        }

        if (Criteria.EndDate is not null && date == Criteria.EndDate)
        {
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(-1);
                return;
            }

            Criteria.EndDate = Criteria.EndDate.Value.AddDays(-14);
        }
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        this.currentBudget = message.Budgets.CurrentActiveBudget;
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

    private void OnWidgetActivatedMessageReceived([NotNull] WidgetActivatedMessage message)
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
