using System.Diagnostics.CodeAnalysis;
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

namespace BudgetAnalyser;

public class ShellController : ControllerBase, IInitializableController
{
    private readonly PersistenceOperations persistenceOperations;
    private readonly IPersistApplicationState statePersistence;
    private readonly IUiContext uiContext;
    private bool initialised;
    private Point originalWindowSize;
    private Point originalWindowTopLeft;

    public ShellController(
        IUiContext uiContext,
        IPersistApplicationState statePersistence,
        PersistenceOperations persistenceOperations)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        if (statePersistence is null)
        {
            throw new ArgumentNullException(nameof(statePersistence));
        }

        if (persistenceOperations is null)
        {
            throw new ArgumentNullException(nameof(persistenceOperations));
        }

        Messenger.Register<ShellController, ShellDialogRequestMessage>(this, static (r, m) => r.OnDialogRequested(m));
        Messenger.Register<ShellController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));
        Messenger.Register<ShellController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m));

        this.statePersistence = statePersistence;
        this.persistenceOperations = persistenceOperations;
        this.uiContext = uiContext;

        LedgerBookDialog = new ShellDialogController(Messenger);
        DashboardDialog = new ShellDialogController(Messenger);
        TransactionsDialog = new ShellDialogController(Messenger);
        BudgetDialog = new ShellDialogController(Messenger);
        ReportsDialog = new ShellDialogController(Messenger);
    }

    [UsedImplicitly]
    public BudgetController BudgetController => this.uiContext.BudgetController;

    public ShellDialogController BudgetDialog { get; }
    public DashboardController DashboardController => this.uiContext.DashboardController;
    public ShellDialogController DashboardDialog { get; }
    public bool HasUnsavedChanges => this.persistenceOperations.HasUnsavedChanges;

    [UsedImplicitly]
    public LedgerBookController LedgerBookController => this.uiContext.LedgerBookController;

    public ShellDialogController LedgerBookDialog { get; }

    [UsedImplicitly]
    public MainMenuController MainMenuController => this.uiContext.MainMenuController;

    [UsedImplicitly]
    public ReportsCatalogController ReportsCatalogController => this.uiContext.ReportsCatalogController;

    public ShellDialogController ReportsDialog { get; }

    [UsedImplicitly]
    public RulesController RulesController => this.uiContext.RulesController;

    [UsedImplicitly]
    public StatementController StatementController => this.uiContext.StatementController;

    public ShellDialogController TransactionsDialog { get; }

    internal Point WindowSize { get; private set; }

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Data binding")]
    [UsedImplicitly]
    public string WindowTitle => "Budget Analyser";

    internal Point WindowTopLeft { get; private set; }

    public void Initialize()
    {
        if (this.initialised)
        {
            return;
        }

        this.uiContext.Logger.LogInfo(_ => $"ShellController Initialise started. {DateTime.Now}");
        this.initialised = true;
        IList<IPersistentApplicationStateObject> rehydratedModels = this.statePersistence.Load()?.ToList();

        if (rehydratedModels is null || rehydratedModels.None())
        {
            rehydratedModels = CreateNewDefaultApplicationState();
        }

        // Create a distinct list of sequences.
        var sequences = rehydratedModels.Select(persistentModel => persistentModel.LoadSequence).OrderBy(s => s).Distinct();

        this.uiContext.Logger.LogInfo(_ => $"ShellController call Initialise on each Controller. {DateTime.Now}");
        this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());

        // Send state load messages in order.
        foreach (var sequence in sequences)
        {
            var sequenceCopy = sequence;
            var models = rehydratedModels.Where(persistentModel => persistentModel.LoadSequence == sequenceCopy);
            this.uiContext.Logger.LogInfo(_ => $"ShellController sending ApplicationStateLoadedMessage for: Sequence{sequence} {models.First().GetType().Name}");
            Messenger.Send(new ApplicationStateLoadedMessage(models));
        }

        this.uiContext.Logger.LogInfo(_ => $"ShellController Initialise completing. Sending ApplicationStateLoadFinishedMessage. {DateTime.Now}");
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
        // Re-run the initialisers. This allows any controller who couldn't initialise until the views are loaded to now reattempt to initialise.
        this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
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
    ///     This method will persist the application state. Application State is user preference settings for the application,
    ///     window, and last loaded file.
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
            var result = this.uiContext.UserPrompts.YesNoBox.Show("There are unsaved changes, save before exiting?", "Budget Analyser");
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
        }

        var storedMainAppState = message.ElementOfType<MainApplicationState>();
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
            TopLeft = WindowTopLeft
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
                LedgerBookDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Dashboard:
                DashboardDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Budget:
                BudgetDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Transactions:
                TransactionsDialog.ShowFromShell(message);
                break;

            case BudgetAnalyserFeature.Reports:
                ReportsDialog.ShowFromShell(message);
                break;

            default:
                throw new NotSupportedException("The requested shell dialog location is not supported: " + message.Location);
        }
    }
}
