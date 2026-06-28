using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class MainMenuController : ControllerBase, IInitializableController
{
    private readonly IUiContext uiContext;

    public MainMenuController(IUiContext uiContext) : base(uiContext.Messenger)
    {
        this.uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        Messenger.Register<MainMenuController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
    }

    [UsedImplicitly]
    public ICommand ShowBudgetCommand => new RelayCommand(OnBudgetExecuted);

    public bool BudgetToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowDashboardCommand => new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand);

    public bool DashboardToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand ShowLedgerBookCommand => new RelayCommand(OnLedgerBookExecuted);

    public bool LedgerBookToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand ShowReportsCommand => new RelayCommand(OnReportsExecuted);

    public bool ReportsToggle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
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

    public void Initialize()
    {
        ShowDashboardCommand.Execute(null);
    }

    private void AfterTabExecutedCommon()
    {
        this.uiContext.Controller<TopDashboardController>().Shown = DashboardToggle;
        this.uiContext.Controller<TopTransactionsListController>().Shown = TransactionsToggle;
        this.uiContext.Controller<TopLedgerBookController>().Shown = LedgerBookToggle;
        this.uiContext.Controller<TopBudgetController>().Shown = BudgetToggle;
        this.uiContext.Controller<TopReportsCatalogController>().Shown = ReportsToggle;
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
