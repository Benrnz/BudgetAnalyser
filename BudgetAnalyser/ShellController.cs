using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Statement;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser
{
    public class ShellController : ControllerBase, IInitializableController
    {
        private readonly IPersistApplicationState statePersistence;
        private readonly UiContext uiContext;
        private bool initialised;

        public ShellController(
            [NotNull] UiContext uiContext,
            [NotNull] IPersistApplicationState statePersistence)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statePersistence == null)
            {
                throw new ArgumentNullException("statePersistence");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShutdownMessage>(this, OnShutdownRequested);
            MessengerInstance.Register<ShellDialogRequestMessage>(this, OnDialogRequested);

            this.statePersistence = statePersistence;
            this.uiContext = uiContext;
            BackgroundJob = uiContext.BackgroundJob;

            LedgerBookDialog = new ShellDialogController();
            DashboardDialog = new ShellDialogController();
            TransactionsDialog = new ShellDialogController();
            BudgetDialog = new ShellDialogController();
            ReportsDialog = new ShellDialogController();
        }

        public IBackgroundProcessingJobMetadata BackgroundJob { get; private set; }

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

        public LedgerBookController LedgerBookController
        {
            get { return this.uiContext.LedgerBookController; }
        }

        public ShellDialogController LedgerBookDialog { get; private set; }

        public MainMenuController MainMenuController
        {
            get { return this.uiContext.MainMenuController; }
        }

        public ReportsCatalogController ReportsCatalogController
        {
            get { return this.uiContext.ReportsCatalogController; }
        }

        public ShellDialogController ReportsDialog { get; private set; }

        public RulesController RulesController
        {
            get { return this.uiContext.RulesController; }
        }

        public StatementController StatementController
        {
            get { return this.uiContext.StatementController; }
        }

        public ShellDialogController TransactionsDialog { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Data binding")]
        public string WindowTitle
        {
            get { return "Budget Analyser"; }
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;

            IList<IPersistent> rehydratedModels = this.statePersistence.Load().ToList();
            if (!rehydratedModels.OfType<LastBudgetLoadedV1>().Any())
            {
                // Mandatory budget file.
                rehydratedModels.Add(new LastBudgetLoadedV1());
            }

            // Create a distinct list of sequences.
            var sequences = rehydratedModels.Select(persistentModel => persistentModel.Sequence).OrderBy(s => s).Distinct();

            // Send state load messages in order.
            foreach (var sequence in sequences)
            {
                int sequenceCopy = sequence;
                MessengerInstance.Send(new ApplicationStateLoadedMessage(rehydratedModels.Where(persistentModel => persistentModel.Sequence == sequenceCopy)));
            }

            MessengerInstance.Send(new ApplicationStateLoadFinishedMessage());

            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        public void OnViewReady()
        {
            // Re-run the initialisers. This allows any controller who couldn't initialise until the views are loaded to now reattempt to initialise.
            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        private void OnDialogRequested(ShellDialogRequestMessage message)
        {
            ShellDialogController appropriateController;
            switch (message.Location)
            {
                case BudgetAnalyserFeature.LedgerBook:
                    appropriateController = LedgerBookDialog;
                    break;

                case BudgetAnalyserFeature.Dashboard:
                    appropriateController = DashboardDialog;
                    break;

                case BudgetAnalyserFeature.Budget:
                    appropriateController = BudgetDialog;
                    break;

                case BudgetAnalyserFeature.Transactions:
                    appropriateController = TransactionsDialog;
                    break;

                case BudgetAnalyserFeature.Reports:
                    appropriateController = ReportsDialog;
                    break;

                default:
                    throw new NotSupportedException("The requested shell dialog location is not supported: " + message.Location);
            }

            appropriateController.Title = message.Title;
            appropriateController.Content = message.Content;
            appropriateController.DialogType = message.DialogType;
            appropriateController.CorrelationId = message.CorrelationId;
        }

        private void OnShutdownRequested(ShutdownMessage message)
        {
            var gatherDataMessage = new ApplicationStateRequestedMessage();
            MessengerInstance.Send(gatherDataMessage);
            this.statePersistence.Persist(gatherDataMessage.PersistentData);
        }
    }
}