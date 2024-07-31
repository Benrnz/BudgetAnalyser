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
using Rees.Wpf;
using Rees.Wpf.Contracts;

// ReSharper disable ClassNeverInstantiated.Global
// Ctor and publics used by bindings.

namespace BudgetAnalyser.Filtering;

[AutoRegisterWithIoC(SingleInstance = true)]
public class GlobalFilterController : ControllerBase, IShellDialogToolTips
{
    private BudgetModel currentBudget;
    private readonly IUserMessageBox userMessageBox;
    private Guid dialogCorrelationId;
    private string doNotUseAccountTypeSummary;
    private GlobalFilterCriteria doNotUseCriteria;
    private string doNotUseDateSummaryLine1;
    private string doNotUseDateSummaryLine2;

    public GlobalFilterController([NotNull] UiContext uiContext)
    {
        if (uiContext == null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.userMessageBox = uiContext.UserPrompts.MessageBox;
        this.doNotUseCriteria = new GlobalFilterCriteria();
        this.currentBudget = uiContext.BudgetController?.CurrentBudget?.Model; //Likely always an empty budget before the bax file is loaded.

        Messenger = uiContext.Messenger;
        Messenger.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        Messenger.Register<ApplicationStateLoadFinishedMessage>(this, OnApplicationStateLoadFinished);
        Messenger.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
        Messenger.Register<RequestFilterMessage>(this, OnGlobalFilterRequested);
        Messenger.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
        Messenger.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        Messenger.Register<RequestFilterChangeMessage>(this, OnGlobalFilterChangeRequested);
        Messenger.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
    }

    public string AccountTypeSummary
    {
        [UsedImplicitly] get => this.doNotUseAccountTypeSummary;
        private set
        {
            this.doNotUseAccountTypeSummary = value;
            OnPropertyChanged();
        }
    }

    public string ActionButtonToolTip => "Apply filter and close.";

    [UsedImplicitly]
    public ICommand AddPeriodCommand => new RelayCommand<DateTime>(OnAddPeriodCommandExecute, d => d != DateTime.MinValue);

    [UsedImplicitly]
    public ICommand BackPeriodCommand => new RelayCommand<DateTime>(OnBackPeriodCommandExecute, d => d != DateTime.MinValue);

    [UsedImplicitly]
    public ICommand ClearCommand => new RelayCommand(OnClearCommandExecute);

    public string CloseButtonToolTip => "Cancel and do not change the filter.";

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
        [UsedImplicitly] get => this.doNotUseDateSummaryLine1;
        private set
        {
            this.doNotUseDateSummaryLine1 = value;
            OnPropertyChanged();
        }
    }

    public string DateSummaryLine2
    {
        [UsedImplicitly] get => this.doNotUseDateSummaryLine2;
        private set
        {
            this.doNotUseDateSummaryLine2 = value;
            OnPropertyChanged();
        }
    }

    public void PromptUserForDates()
    {
        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Global Date Filter"
        };
        Messenger.Send(dialogRequest);
    }

    private void OnAddPeriodCommandExecute(DateTime date)
    {
        if (Criteria.BeginDate != null && date == Criteria.BeginDate)
        {
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(14);
        }

        if (Criteria.EndDate != null && date == Criteria.EndDate)
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
        if (filterState == null)
        {
            return;
        }

        Criteria = new GlobalFilterCriteria
        {
            BeginDate = filterState.BeginDate,
            EndDate = filterState.EndDate
        };

        SendFilterAppliedMessage();
    }

    private void OnApplicationStateLoadFinished(ApplicationStateLoadFinishedMessage message)
    {
        if (Criteria == null || Criteria.Cleared)
        {
            SendFilterAppliedMessage();
        }
    }

    private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
    {
        var noCriteria = Criteria == null;
        var filterState = new PersistentFiltersApplicationState
        {
            BeginDate = noCriteria ? null : Criteria.BeginDate,
            EndDate = noCriteria ? null : Criteria.EndDate
        };

        message.PersistThisModel(filterState);
    }

    private void OnBackPeriodCommandExecute(DateTime date)
    {
        if (Criteria.BeginDate != null && date == Criteria.BeginDate)
        {
            if (this.currentBudget.BudgetCycle == BudgetCycle.Monthly)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(-1);
                return;
            }

            Criteria.BeginDate = Criteria.BeginDate.Value.AddDays(-14);
        }

        if (Criteria.EndDate != null && date == Criteria.EndDate)
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
        if (message == null)
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

        if (Criteria.BeginDate != null)
        {
            DateSummaryLine1 = string.Format(CultureInfo.CurrentCulture, "Filtered from: {0:d}", Criteria.BeginDate.Value);
        }

        if (Criteria.EndDate != null)
        {
            DateSummaryLine2 = string.Format(CultureInfo.CurrentCulture, "up until: {0:d}", Criteria.EndDate.Value);
        }
    }
}