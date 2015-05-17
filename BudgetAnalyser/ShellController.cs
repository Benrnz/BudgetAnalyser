using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Statement;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser
{
    public class ShellController : ControllerBase, IInitializableController
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly IDashboardService dashboardService;
        private readonly IPersistApplicationState statePersistence;
        private readonly IUiContext uiContext;
        private bool initialised;
        private Point originalWindowSize;
        private Point originalWindowTopLeft;

        public ShellController(
            [NotNull] IUiContext uiContext,
            [NotNull] IPersistApplicationState statePersistence,
            [NotNull] IApplicationDatabaseService applicationDatabaseService,
            [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statePersistence == null)
            {
                throw new ArgumentNullException("statePersistence");
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException("applicationDatabaseService");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShutdownMessage>(this, OnShutdownRequested);
            MessengerInstance.Register<ShellDialogRequestMessage>(this, OnDialogRequested);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);

            this.statePersistence = statePersistence;
            this.applicationDatabaseService = applicationDatabaseService;
            this.dashboardService = dashboardService;
            this.uiContext = uiContext;

            LedgerBookDialog = new ShellDialogController();
            DashboardDialog = new ShellDialogController();
            TransactionsDialog = new ShellDialogController();
            BudgetDialog = new ShellDialogController();
            ReportsDialog = new ShellDialogController();
        }

        [Engine.Annotations.UsedImplicitly]
        public BudgetController BudgetController
        {
            get { return this.uiContext.BudgetController; }
        }

        public ShellDialogController BudgetDialog { get; private set; }

        public DashboardController DashboardController
        {
            get { return this.uiContext.DashboardController; }
        }

        public ShellDialogController DashboardDialog { get; private set; }

        [Engine.Annotations.UsedImplicitly]
        public LedgerBookController LedgerBookController
        {
            get { return this.uiContext.LedgerBookController; }
        }

        public ShellDialogController LedgerBookDialog { get; private set; }

        [Engine.Annotations.UsedImplicitly]
        public MainMenuController MainMenuController
        {
            get { return this.uiContext.MainMenuController; }
        }

        [Engine.Annotations.UsedImplicitly]
        public ReportsCatalogController ReportsCatalogController
        {
            get { return this.uiContext.ReportsCatalogController; }
        }

        public ShellDialogController ReportsDialog { get; private set; }

        [Engine.Annotations.UsedImplicitly]
        public RulesController RulesController
        {
            get { return this.uiContext.RulesController; }
        }

        [Engine.Annotations.UsedImplicitly]
        public StatementController StatementController
        {
            get { return this.uiContext.StatementController; }
        }

        public ShellDialogController TransactionsDialog { get; private set; }
        internal Point WindowSize { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Data binding")]
        [Engine.Annotations.UsedImplicitly]
        public string WindowTitle
        {
            get { return "Budget Analyser"; }
        }

        internal Point WindowTopLeft { get; private set; }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;

            IList<IPersistent> rehydratedModels = this.statePersistence.Load().ToList();

            // Create a distinct list of sequences.
            IEnumerable<int> sequences = rehydratedModels.Select(persistentModel => persistentModel.LoadSequence).OrderBy(s => s).Distinct();

            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());

            // Send state load messages in order.
            foreach (int sequence in sequences)
            {
                int sequenceCopy = sequence;
                IEnumerable<IPersistent> models = rehydratedModels.Where(persistentModel => persistentModel.LoadSequence == sequenceCopy);
                MessengerInstance.Send(new ApplicationStateLoadedMessage(models));
            }

            MessengerInstance.Send(new ApplicationStateLoadFinishedMessage());
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

        private async void OnApplicationStateLoaded([NotNull] ApplicationStateLoadedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var shellState = message.ElementOfType<ShellPersistentStateV1>();
            if (shellState != null)
            {
                // Setting Window Size at this point has no effect, must happen after window is loaded. See OnViewReady()
                if (shellState.Size.X > 0 || shellState.Size.Y > 0)
                {
                    this.originalWindowSize = shellState.Size;
                }
                else
                {
                    this.originalWindowSize = new Point(1250, 600);
                }

                if (shellState.TopLeft.X > 0 || shellState.TopLeft.Y > 0)
                {
                    // Setting Window Top & Left at this point has no effect, must happen after window is loaded. See OnViewReady()
                    this.originalWindowTopLeft = shellState.TopLeft;
                }
            }

            var storedMainAppState = message.ElementOfType<MainApplicationStateModelV1>();
            if (storedMainAppState != null)
            {
                ApplicationDatabase applicationDatabase = await this.applicationDatabaseService.LoadAsync(storedMainAppState.BudgetAnalyserDataStorageKey);
                MessengerInstance.Send(new ApplicationDatabaseReadyMessage(applicationDatabase));
                this.dashboardService.NotifyOfDependencyChange<ApplicationDatabase>(applicationDatabase);
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var shellPersistentStateV1 = new ShellPersistentStateV1
            {
                Size = WindowSize,
                TopLeft = WindowTopLeft
            };
            message.PersistThisModel(shellPersistentStateV1);

            MainApplicationStateModelV1 dataFileState = this.applicationDatabaseService.PreparePersistentStateData();
            message.PersistThisModel(dataFileState);
        }

        private void OnDialogRequested(ShellDialogRequestMessage message)
        {
            ShellDialogController dialogController;
            switch (message.Location)
            {
                case BudgetAnalyserFeature.LedgerBook:
                    dialogController = LedgerBookDialog;
                    break;

                case BudgetAnalyserFeature.Dashboard:
                    dialogController = DashboardDialog;
                    break;

                case BudgetAnalyserFeature.Budget:
                    dialogController = BudgetDialog;
                    break;

                case BudgetAnalyserFeature.Transactions:
                    dialogController = TransactionsDialog;
                    break;

                case BudgetAnalyserFeature.Reports:
                    dialogController = ReportsDialog;
                    break;

                default:
                    throw new NotSupportedException("The requested shell dialog location is not supported: " + message.Location);
            }

            dialogController.Title = message.Title;
            dialogController.Content = message.Content;
            dialogController.DialogType = message.DialogType;
            dialogController.CorrelationId = message.CorrelationId;
            dialogController.HelpButtonVisible = message.HelpAvailable;
        }

        private void OnShutdownRequested(ShutdownMessage message)
        {
            var gatherDataMessage = new ApplicationStateRequestedMessage();
            MessengerInstance.Send(gatherDataMessage);
            this.statePersistence.Persist(gatherDataMessage.PersistentData);

            if (this.applicationDatabaseService.HasUnsavedChanges)
            {
                bool? response = this.uiContext.UserPrompts.YesNoBox.Show("Save changes before exiting?");
                if (response != null && response.Value)
                {
                    // Must be carefully run because the application is exiting.  If run using the task factory with defaults the task will stall, as its waiting to be marshalled back to main context
                    // which is waiting here.  If run without a .Wait(), the task will be aborted by the shutdown and the file is cut short and partially saved; actually corrupted.
                    var t = new Task(() => this.applicationDatabaseService.SaveAsync());
                    t.ConfigureAwait(false);
                    t.Start();
                    t.Wait();
                }
            }
        }
    }
}