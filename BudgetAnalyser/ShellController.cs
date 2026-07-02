using System.Windows;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class ShellController : ControllerBase
{
    private readonly PersistenceOperations persistenceOperations;
    private readonly IPersistApplicationState statePersistence;
    private readonly IUserQuestionBoxYesNo yesNoMessageBox;

    public ShellController(
        IMessenger messenger,
        UserPrompts userPrompts,
        MainMenuController mainMenuController,
        TopBudgetController budgetController,
        TopDashboardController dashboardController,
        TopLedgerBookController ledgerBookController,
        TopReportsCatalogController reportsCatalogController,
        TopTransactionsListController transactionsController,
        IPersistApplicationState statePersistence,
        PersistenceOperations persistenceOperations)
        : base(messenger)
    {
        this.statePersistence = statePersistence ?? throw new ArgumentNullException(nameof(statePersistence));
        this.persistenceOperations = persistenceOperations ?? throw new ArgumentNullException(nameof(persistenceOperations));
        this.yesNoMessageBox = userPrompts.YesNoBox ?? throw new ArgumentNullException(nameof(userPrompts.YesNoBox));
        MainMenuController = mainMenuController ?? throw new ArgumentNullException(nameof(mainMenuController));
        TopBudgetController = budgetController ?? throw new ArgumentNullException(nameof(budgetController));
        TopDashboardController = dashboardController ?? throw new ArgumentNullException(nameof(dashboardController));
        TopLedgerBookController = ledgerBookController ?? throw new ArgumentNullException(nameof(ledgerBookController));
        TopReportsCatalogController = reportsCatalogController ?? throw new ArgumentNullException(nameof(reportsCatalogController));
        TopTransactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));

        Messenger.Register<ShellController, ShellDialogRequestMessage>(this, static (r, m) => r.OnDialogRequested(m));
        Messenger.Register<ShellController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));
        Messenger.Register<ShellController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m));

        LedgerBookTabDialog = new ShellDialogController(Messenger);
        DashboardTabDialog = new ShellDialogController(Messenger);
        TransactionsTabDialog = new ShellDialogController(Messenger);
        BudgetTabDialog = new ShellDialogController(Messenger);
        ReportsTabDialog = new ShellDialogController(Messenger);
    }

    public ShellDialogController BudgetTabDialog { get; }
    public ShellDialogController DashboardTabDialog { get; }
    public bool HasUnsavedChanges => this.persistenceOperations.HasUnsavedChanges;
    public ShellDialogController LedgerBookTabDialog { get; }
    public MainMenuController MainMenuController { get; }
    public ShellDialogController ReportsTabDialog { get; }
    public TopBudgetController TopBudgetController { get; }
    public TopDashboardController TopDashboardController { get; }
    public TopLedgerBookController TopLedgerBookController { get; }
    public TopReportsCatalogController TopReportsCatalogController { get; }
    public TopTransactionsListController TopTransactionsController { get; }
    public ShellDialogController TransactionsTabDialog { get; }
    internal Point WindowSize { get; set; }
    public string WindowTitle => "Budget Analyser";
    internal Point WindowTopLeft { get; set; }

    internal void NotifyOfWindowLocationChange(Point location)
    {
        WindowTopLeft = location;
    }

    internal void NotifyOfWindowSizeChange(Point size)
    {
        WindowSize = size;
    }

    /// <summary>
    ///     This method will persist the application state. Application State is user preference settings for the application, window, and last loaded file.
    ///     Any data that is used for Budgets, reconciliation, reporting belongs in the Application Database.
    /// </summary>
    internal void SaveApplicationState()
    {
        var gatherDataMessage = new ApplicationStateRequestedMessage();
        Messenger.Send(gatherDataMessage);
        this.statePersistence.Persist(gatherDataMessage.PersistentData);
    }

    /// <summary>
    ///     Notify the ShellController the Shell is closing.
    /// </summary>
    internal async Task<bool> ShellClosing()
    {
        if (this.persistenceOperations.HasUnsavedChanges)
        {
            var result = this.yesNoMessageBox.Show("There are unsaved changes, save before exiting?", "Budget Analyser");
            if (result is not null && result.Value)
            {
                // Save must be run carefully because the application is exiting.  If run using the task factory with defaults the task will stall, as background tasks are waiting to be marshalled back to main context
                // which is also waiting here, resulting in a deadlock.  This method will only work by first cancelling the close, awaiting this method and then re-triggering it.
                await this.persistenceOperations.SaveDatabase();
            }

            return true;
        }

        return false;
    }

    private async void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var shellState = message.ElementOfType<ShellPersistentState>();
        if (shellState is not null)
        {
            // Setting Window Size at this point has no effect, must happen after window is loaded. Handled by the view.
            WindowSize = shellState.Size.X > 0 || shellState.Size.Y > 0 ? shellState.Size : new Point(1250, 600);

            if (shellState.TopLeft.X > 0 || shellState.TopLeft.Y > 0)
            {
                // Setting Window Top & Left at this point has no effect, must happen after window is loaded.  Handled by the view.
                WindowTopLeft = shellState.TopLeft;
            }

            TopTransactionsController.PageSize = shellState.ListPageSize;
        }

        // Todo this should move to DashboardController
        var storedMainAppState = message.ElementOfType<ApplicationEngineState>();
        if (storedMainAppState is not null)
        {
            await this.persistenceOperations.LoadDatabase(storedMainAppState.BudgetAnalyserDataStorageKey);
        }
    }

    private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
    {
        var shellPersistentStateV1 = new ShellPersistentState
        {
            Size = WindowSize,
            TopLeft = WindowTopLeft,
            ListPageSize = TopTransactionsController.PageSize
        };
        message.PersistThisModel(shellPersistentStateV1);

        // Todo this should move to DashboardController
        var dataFileState = this.persistenceOperations.PreparePersistentStateData();
        message.PersistThisModel(dataFileState);
    }

    private void OnDialogRequested(ShellDialogRequestMessage message)
    {
        // Each mega-tab has its own dialog controller.  This is so each area can have independent dialogs, allowing the user to refer to other mega-tabs while a dialog is open.
        switch (message.Location)
        {
            case BudgetAnalyserFeature.LedgerBook:
                LedgerBookTabDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Dashboard:
                DashboardTabDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Budget:
                BudgetTabDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Transactions:
                TransactionsTabDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Reports:
                ReportsTabDialog.ShowFromShell(message);
                break;

            default:
                throw new NotSupportedException("The requested shell dialog location is not supported: " + message.Location);
        }
    }
}
