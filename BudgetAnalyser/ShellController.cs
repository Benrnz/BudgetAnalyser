using System.Windows;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class ShellController : ControllerBase, IInitializableController
{
    private readonly PersistenceOperations persistenceOperations;
    private readonly IPersistApplicationState statePersistence;
    private readonly IUserQuestionBoxYesNo userPromptToSave;
    private bool initialised;
    private Point originalWindowSize;
    private Point originalWindowTopLeft;
    private readonly ILogger logger;
    private readonly IInitializableController[] initializableControllers;

    public ShellController(
        IUiContext uiContext,
        IPersistApplicationState statePersistence,
        PersistenceOperations persistenceOperations,
        MainMenuController mainMenuController,
        TabDashboardController dashboardController,
        TabTransactionsController transactionsController,
        TabLedgerBookController ledgerBookController,
        TabBudgetController budgetController,
        TabReportsCatalogController reportsCatalogController,
        RulesController rulesController)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.logger = uiContext.Logger;
        this.userPromptToSave = uiContext.UserPrompts.YesNoBox;
        // TODO Ideally this list of InitializableControllers should be passed in from DI.
        this.initializableControllers = uiContext.Controllers.OfType<IInitializableController>().ToArray();
        this.statePersistence = statePersistence ?? throw new ArgumentNullException(nameof(statePersistence));
        this.persistenceOperations = persistenceOperations ?? throw new ArgumentNullException(nameof(persistenceOperations));
        MainMenuController = mainMenuController ?? throw new ArgumentNullException(nameof(mainMenuController));
        TabDashboardController = dashboardController ?? throw new ArgumentNullException(nameof(dashboardController));
        TabTransactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));
        TabLedgerBookController = ledgerBookController ?? throw new ArgumentNullException(nameof(ledgerBookController));
        TabBudgetController = budgetController ?? throw new ArgumentNullException(nameof(budgetController));
        TabReportsCatalogController = reportsCatalogController ?? throw new ArgumentNullException(nameof(reportsCatalogController));

        // TODO Ideally should change so that Rules Controller is owned and managed by the TabTransactionsController.
        RulesController = rulesController ?? throw new ArgumentNullException(nameof(rulesController));

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

    public MainMenuController MainMenuController { get; private set; }
    public ShellDialogController ReportsTabDialog { get; }
    public RulesController RulesController { get; private set; }

    public TabBudgetController TabBudgetController { get; private set; }
    public TabDashboardController TabDashboardController { get; private set; }
    public TabLedgerBookController TabLedgerBookController { get; private set; }
    public TabReportsCatalogController TabReportsCatalogController { get; private set; }
    public TabTransactionsController TabTransactionsController { get; private set; }
    public ShellDialogController TransactionsTabDialog { get; }
    internal Point WindowSize { get; private set; }
    public string WindowTitle => "Budget Analyser";
    internal Point WindowTopLeft { get; private set; }

    public void Initialize()
    {
        if (this.initialised)
        {
            return;
        }

        this.logger.LogInfo(_ => $"ShellController Initialise started. {DateTime.Now}");
        this.initialised = true;
        IList<IPersistentApplicationStateObject> rehydratedModels = this.statePersistence.Load().ToList();

        if (rehydratedModels.None())
        {
            rehydratedModels = CreateNewDefaultApplicationState();
        }

        // Create a distinct list of sequences.
        var sequences = rehydratedModels.Select(persistentModel => persistentModel.LoadSequence).OrderBy(s => s).Distinct();

        this.logger.LogInfo(_ => $"ShellController call Initialise on each Controller. {DateTime.Now}");
        Array.ForEach(this.initializableControllers, i => i.Initialize());

        // Send state load messages in order.
        foreach (var sequence in sequences)
        {
            var sequenceCopy = sequence;
            var models = rehydratedModels.Where(persistentModel => persistentModel.LoadSequence == sequenceCopy);
            this.logger.LogInfo(_ => $"ShellController sending ApplicationStateLoadedMessage for: Sequence{sequence} {models.First().GetType().Name}");
            Messenger.Send(new ApplicationStateLoadedMessage(models));
        }

        this.logger.LogInfo(_ => $"ShellController Initialise completing. Sending ApplicationStateLoadFinishedMessage. {DateTime.Now}");
        Messenger.Send(new ApplicationStateLoadFinishedMessage());
    }

    public void NotifyOfWindowLocationChange(Point location)
    {
        WindowTopLeft = location;
    }

    public void NotifyOfWindowSizeChange(Point size)
    {
        WindowSize = size;
    }

    public void OnViewReady()
    {
        // Re-run the initializers. This allows any controller who couldn't initialise until the views are loaded to now reattempt to initialise.
        if (this.originalWindowTopLeft != new Point())
        {
            WindowTopLeft = this.originalWindowTopLeft;
        }

        if (this.originalWindowSize != new Point())
        {
            WindowSize = this.originalWindowSize;
        }
    }

    /// <summary>
    ///     This method will persist the application state. Application State is user preference settings for the application, window, and last loaded file.
    ///     Any data that is used for Budgets, reconciliation, reporting belongs in the Application Database.
    /// </summary>
    public void SaveApplicationState()
    {
        var gatherDataMessage = new ApplicationStateRequestedMessage();
        Messenger.Send(gatherDataMessage);
        this.statePersistence.Persist(gatherDataMessage.PersistentData);
    }

    /// <summary>
    ///     Notify the ShellController the Shell is closing.
    /// </summary>
    public async Task<bool> ShellClosing()
    {
        if (this.persistenceOperations.HasUnsavedChanges)
        {
            var result = this.userPromptToSave.Show("There are unsaved changes, save before exiting?", "Budget Analyser");
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

    private static IList<IPersistentApplicationStateObject> CreateNewDefaultApplicationState()
    {
        var appState = new List<IPersistentApplicationStateObject>();
        return appState;
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
            // Setting Window Size at this point has no effect, must happen after window is loaded. See OnViewReady()
            this.originalWindowSize = shellState.Size.X > 0 || shellState.Size.Y > 0 ? shellState.Size : new Point(1250, 600);

            if (shellState.TopLeft.X > 0 || shellState.TopLeft.Y > 0)
            {
                // Setting Window Top & Left at this point has no effect, must happen after window is loaded. See OnViewReady()
                this.originalWindowTopLeft = shellState.TopLeft;
            }

            TabTransactionsController.PageSize = shellState.ListPageSize;
        }

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
            ListPageSize = TabTransactionsController.PageSize
        };
        message.PersistThisModel(shellPersistentStateV1);

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
