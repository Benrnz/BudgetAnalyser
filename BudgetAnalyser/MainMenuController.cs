using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
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
        ShowBudgetCommand = new RelayCommand(OnBudgetExecuted);
        ShowDashboardCommand = new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand);
        ShowLedgerBookCommand = new RelayCommand(OnLedgerBookExecuted);
        ShowReportsCommand = new RelayCommand(OnReportsExecuted);
        ShowTransactionsCommand = new RelayCommand(OnTransactionExecuted);

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

    public IRelayCommand ShowBudgetCommand { get; }

    public IRelayCommand ShowDashboardCommand { get; }

    public IRelayCommand ShowLedgerBookCommand { get; }

    public IRelayCommand ShowReportsCommand { get; }

    public IRelayCommand ShowTransactionsCommand { get; }

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
}
