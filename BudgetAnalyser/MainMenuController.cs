using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class MainMenuController : ControllerBase
{
    private readonly TopBudgetController budgetController;
    private readonly TopDashboardController dashboardController;
    private readonly TopLedgerBookController ledgerBookController;
    private readonly TopReportsCatalogController reportsCatalogController;
    private readonly TopTransactionsListController transactionsController;

    public MainMenuController(
        TopDashboardController dashboardController,
        TopTransactionsListController transactionsController,
        TopLedgerBookController ledgerBookController,
        TopBudgetController budgetController,
        TopReportsCatalogController reportsCatalogController,
        IMessenger messenger) : base(messenger)
    {
        this.dashboardController = dashboardController ?? throw new ArgumentNullException(nameof(dashboardController));
        this.transactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));
        this.ledgerBookController = ledgerBookController ?? throw new ArgumentNullException(nameof(ledgerBookController));
        this.budgetController = budgetController ?? throw new ArgumentNullException(nameof(budgetController));
        this.reportsCatalogController = reportsCatalogController ?? throw new ArgumentNullException(nameof(reportsCatalogController));
        Messenger.Register<MainMenuController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));

        // Default visible tab is the Dashboard tab
        OnDashboardExecuted();
    }

    public bool BudgetToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool DashboardToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool LedgerBookToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool ReportsToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowBudgetCommand => new RelayCommand(OnBudgetExecuted);

    public ICommand ShowDashboardCommand => new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand);

    public ICommand ShowLedgerBookCommand => new RelayCommand(OnLedgerBookExecuted);

    public ICommand ShowReportsCommand => new RelayCommand(OnReportsExecuted);

    public ICommand ShowTransactionsCommand => new RelayCommand(OnTransactionExecuted);

    public bool TransactionsToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
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

        // Could possibily go direct to PersistenceOperation class here.
        PersistenceOperationCommands.LoadDemoDatabaseCommand.Execute(this);
    }
}
