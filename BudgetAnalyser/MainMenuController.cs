using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class MainMenuController : ControllerBase, IInitializableController
{
    private readonly TabBudgetController budgetController;
    private readonly TabDashboardController dashboardController;
    private readonly TabLedgerBookController ledgerBookController;
    private readonly TabReportsCatalogController reportsCatalogController;
    private readonly TabTransactionsController transactionsController;
    private bool doNotUseBudgetToggle;
    private bool doNotUseDashboardToggle;
    private bool doNotUseLedgerBookToggle;
    private bool doNotUseReportsToggle;
    private bool doNotUseTransactionsToggle;

    public MainMenuController(
        IUiContext uiContext,
        TabDashboardController dashboardController,
        TabTransactionsController transactionsController,
        TabLedgerBookController ledgerBookController,
        TabBudgetController budgetController,
        TabReportsCatalogController reportsCatalogController)
        : base(uiContext.Messenger)
    {
        this.dashboardController = dashboardController ?? throw new ArgumentNullException(nameof(dashboardController));
        this.transactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));
        this.ledgerBookController = ledgerBookController ?? throw new ArgumentNullException(nameof(ledgerBookController));
        this.budgetController = budgetController ?? throw new ArgumentNullException(nameof(budgetController));
        this.reportsCatalogController = reportsCatalogController ?? throw new ArgumentNullException(nameof(reportsCatalogController));
        Messenger.Register<MainMenuController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
    }

    public bool BudgetToggle
    {
        get => this.doNotUseBudgetToggle;
        set
        {
            this.doNotUseBudgetToggle = value;
            OnPropertyChanged();
        }
    }

    public bool DashboardToggle
    {
        get => this.doNotUseDashboardToggle;
        set
        {
            this.doNotUseDashboardToggle = value;
            OnPropertyChanged();
        }
    }

    public bool LedgerBookToggle
    {
        get => this.doNotUseLedgerBookToggle;
        set
        {
            this.doNotUseLedgerBookToggle = value;
            OnPropertyChanged();
        }
    }

    public bool ReportsToggle
    {
        get => this.doNotUseReportsToggle;
        set
        {
            this.doNotUseReportsToggle = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowBudgetTabCommand => new RelayCommand(OnShowBudgetTabExecuted);

    public ICommand ShowDashboardTabCommand => new RelayCommand(OnShowDashboardTabExecuted, CanExecuteDashboardCommand);

    public ICommand ShowLedgerBookTabCommand => new RelayCommand(OnShowLedgerBookTabExecuted);

    public ICommand ShowReportsTabCommand => new RelayCommand(OnShowReportsTabExecuted);

    public ICommand ShowTransactionsTabCommand => new RelayCommand(OnShowTransactionsTabExecuted);

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
        ShowDashboardTabCommand.Execute(null);
    }

    private void AfterTabExecutedCommon()
    {
        this.dashboardController.Shown = DashboardToggle;
        this.transactionsController.Shown = TransactionsToggle;
        this.ledgerBookController.Shown = LedgerBookToggle;
        this.budgetController.Shown = BudgetToggle;
        this.reportsCatalogController.Shown = ReportsToggle;
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

    private void OnShowBudgetTabExecuted()
    {
        BeforeTabExecutedCommon();
        BudgetToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnShowDashboardTabExecuted()
    {
        BeforeTabExecutedCommon();
        DashboardToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnShowLedgerBookTabExecuted()
    {
        BeforeTabExecutedCommon();
        LedgerBookToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnShowReportsTabExecuted()
    {
        BeforeTabExecutedCommon();
        ReportsToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnShowTransactionsTabExecuted()
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
            OnShowTransactionsTabExecuted();
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
        if (message.Widget is not NewFileWidget)
        {
            return;
        }

        message.Handled = true;

        PersistenceOperationCommands.CreateNewDatabaseCommand.Execute(this);
    }

    private void ProcessCurrentFileWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not CurrentFileWidget)
        {
            return;
        }

        message.Handled = true;

        PersistenceOperationCommands.LoadDatabaseCommand.Execute(this);
    }

    private void ProcessLoadDemoWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not LoadDemoWidget)
        {
            return;
        }

        message.Handled = true;

        PersistenceOperationCommands.LoadDemoDatabaseCommand.Execute(this);
    }
}
