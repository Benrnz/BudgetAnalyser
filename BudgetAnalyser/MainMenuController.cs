using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Settings;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public partial class MainMenuController : ControllerBase
{
    private readonly TopBudgetController budgetController;
    private readonly TopDashboardController dashboardController;
    private readonly TopLedgerBookController ledgerBookController;
    private readonly TopReportsCatalogController reportsCatalogController;
    private readonly TopSettingsController settingsController;
    private readonly TopTransactionsListController transactionsController;

    public MainMenuController(
        TopDashboardController dashboardController,
        TopTransactionsListController transactionsController,
        TopLedgerBookController ledgerBookController,
        TopBudgetController budgetController,
        TopReportsCatalogController reportsCatalogController,
        TopSettingsController settingsController,
        IMessenger messenger) : base(messenger)
    {
        this.dashboardController = dashboardController ?? throw new ArgumentNullException(nameof(dashboardController));
        this.transactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));
        this.ledgerBookController = ledgerBookController ?? throw new ArgumentNullException(nameof(ledgerBookController));
        this.budgetController = budgetController ?? throw new ArgumentNullException(nameof(budgetController));
        this.reportsCatalogController = reportsCatalogController ?? throw new ArgumentNullException(nameof(reportsCatalogController));
        this.settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
        ShowBudgetCommand = new RelayCommand(OnBudgetExecuted);
        ShowDashboardCommand = new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand);
        ShowLedgerBookCommand = new RelayCommand(OnLedgerBookExecuted);
        ShowReportsCommand = new RelayCommand(OnReportsExecuted);
        ShowSettingsCommand = new RelayCommand(OnSettingsExecuted);
        ShowTransactionsCommand = new RelayCommand(OnTransactionExecuted);

        Messenger.Register<MainMenuController, MainMenuTabRequestMessage>(this, OnTabRequested);

        // Default visible tab is the Dashboard tab
        OnDashboardExecuted();
    }

    [ObservableProperty]
    public partial bool BudgetToggle { get; set; }

    [ObservableProperty]
    public partial bool DashboardToggle { get; set; }

    [ObservableProperty]
    public partial bool LedgerBookToggle { get; set; }

    [ObservableProperty]
    public partial bool ReportsToggle { get; set; }

    [ObservableProperty]
    public partial bool SettingsToggle { get; set; }

    public IRelayCommand ShowBudgetCommand { get; }

    public IRelayCommand ShowDashboardCommand { get; }

    public IRelayCommand ShowLedgerBookCommand { get; }

    public IRelayCommand ShowReportsCommand { get; }

    public IRelayCommand ShowSettingsCommand { get; }

    public IRelayCommand ShowTransactionsCommand { get; }

    [ObservableProperty]
    public partial bool TransactionsToggle { get; set; }

    private void AfterTabExecutedCommon()
    {
        this.dashboardController.Shown = DashboardToggle;
        this.transactionsController.Shown = TransactionsToggle;
        this.ledgerBookController.Shown = LedgerBookToggle;
        this.budgetController.Shown = BudgetToggle;
        this.reportsCatalogController.Shown = ReportsToggle;
        this.settingsController.Shown = SettingsToggle;
    }

    private void BeforeTabExecutedCommon()
    {
        DashboardToggle = false;
        TransactionsToggle = false;
        LedgerBookToggle = false;
        BudgetToggle = false;
        ReportsToggle = false;
        SettingsToggle = false;
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

    private void OnSettingsExecuted()
    {
        BeforeTabExecutedCommon();
        SettingsToggle = true;
        AfterTabExecutedCommon();
    }

    private void OnTabRequested(MainMenuController recipient, MainMenuTabRequestMessage message)
    {
        switch (message.Tab)
        {
            case MainMenuTab.Dashboard:
                OnDashboardExecuted();
                break;
            case MainMenuTab.Transactions:
                OnTransactionExecuted();
                break;
            case MainMenuTab.Ledger:
                OnLedgerBookExecuted();
                break;
            case MainMenuTab.Budget:
                OnBudgetExecuted();
                break;
            case MainMenuTab.Reports:
                OnReportsExecuted();
                break;
            case MainMenuTab.Settings:
                OnSettingsExecuted();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnTransactionExecuted()
    {
        BeforeTabExecutedCommon();
        TransactionsToggle = true;
        AfterTabExecutedCommon();
    }
}
