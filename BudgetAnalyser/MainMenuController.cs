using System.Windows.Input;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class MainMenuController : ControllerBase, IInitializableController
{
    private readonly IUiContext uiContext;
    private bool doNotUseBudgetToggle;
    private bool doNotUseDashboardToggle;
    private bool doNotUseLedgerBookToggle;
    private bool doNotUseReportsToggle;
    private bool doNotUseTransactionsToggle;

    public MainMenuController(
        IUiContext uiContext,
        IDashboardService dashboardService,
        DemoFileHelper demoFileHelper)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        if (dashboardService is null)
        {
            throw new ArgumentNullException(nameof(dashboardService));
        }

        if (demoFileHelper is null)
        {
            throw new ArgumentNullException(nameof(demoFileHelper));
        }

        this.uiContext = uiContext;
        Messenger.Register<MainMenuController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
    }

    [UsedImplicitly]
    public ICommand BudgetCommand => new RelayCommand(OnBudgetExecuted);

    public bool BudgetToggle
    {
        get => this.doNotUseBudgetToggle;
        set
        {
            this.doNotUseBudgetToggle = value;
            OnPropertyChanged();
        }
    }

    public ICommand DashboardCommand => new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand);

    public bool DashboardToggle
    {
        get => this.doNotUseDashboardToggle;
        set
        {
            this.doNotUseDashboardToggle = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand LedgerBookCommand => new RelayCommand(OnLedgerBookExecuted);

    public bool LedgerBookToggle
    {
        get => this.doNotUseLedgerBookToggle;
        set
        {
            this.doNotUseLedgerBookToggle = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand ReportsCommand => new RelayCommand(OnReportsExecuted);

    public bool ReportsToggle
    {
        get => this.doNotUseReportsToggle;
        set
        {
            this.doNotUseReportsToggle = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand TransactionsCommand => new RelayCommand(OnTransactionExecuted);

    public bool TransactionsToggle
    {
        get => this.doNotUseTransactionsToggle;
        set
        {
            this.doNotUseTransactionsToggle = value;
            OnPropertyChanged();
        }
    }

    public void Initialize()
    {
        DashboardCommand.Execute(null);
    }

    private void AfterTabExecutedCommon()
    {
        foreach (var controller in this.uiContext.ShowableControllers)
        {
            controller.Shown = false;
        }

        this.uiContext.DashboardController.Shown = DashboardToggle;
        this.uiContext.StatementController.Shown = TransactionsToggle;
        this.uiContext.LedgerBookController.Shown = LedgerBookToggle;
        this.uiContext.BudgetController.Shown = BudgetToggle;
        this.uiContext.ReportsCatalogController.Shown = ReportsToggle;
    }

    private void BeforeTabExecutedCommon()
    {
        DashboardToggle = false;
        TransactionsToggle = false;
        LedgerBookToggle = false;
        BudgetToggle = false;
        ReportsToggle = false;
    }

    private static bool CanExecuteDashboardCommand()
    {
        return true;
    }

    private void OnBudgetExecuted()
    {
        BeforeTabExecutedCommon();
        BudgetToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnDashboardExecuted()
    {
        BeforeTabExecutedCommon();
        DashboardToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnLedgerBookExecuted()
    {
        BeforeTabExecutedCommon();
        LedgerBookToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnReportsExecuted()
    {
        BeforeTabExecutedCommon();
        ReportsToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnTransactionExecuted()
    {
        BeforeTabExecutedCommon();
        TransactionsToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (message.Handled)
        {
            return;
        }

        if (message.Widget is SaveWidget)
        {
            if (PersistenceOperationCommands.SaveDatabaseCommand.CanExecute(null))
            {
                PersistenceOperationCommands.SaveDatabaseCommand.Execute(null);
            }

            return;
        }

        if (message.Widget is DaysSinceLastImport)
        {
            OnTransactionExecuted();
            return;
        }

        if (message.Widget is CurrentFileWidget)
        {
            ProcessCurrentFileWidgetActivated(message);
            return;
        }

        if (message.Widget is LoadDemoWidget)
        {
            ProcessLoadDemoWidgetActivated(message);
            return;
        }

        if (message.Widget is NewFileWidget)
        {
            ProcessCreateNewFileWidgetActivated(message);
        }
    }

    private void ProcessCreateNewFileWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not NewFileWidget widget)
        {
            return;
        }

        message.Handled = true;

        PersistenceOperationCommands.CreateNewDatabaseCommand.Execute(this);
    }

    private void ProcessCurrentFileWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not CurrentFileWidget widget)
        {
            return;
        }

        message.Handled = true;

        PersistenceOperationCommands.LoadDatabaseCommand.Execute(this);
    }

    private void ProcessLoadDemoWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not LoadDemoWidget widget)
        {
            return;
        }

        message.Handled = true;

        // Could possibily go direct to PersistenceOperation class here.
        PersistenceOperationCommands.LoadDemoDatabaseCommand.Execute(this);
    }
}
