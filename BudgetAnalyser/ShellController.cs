using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser
{
    public class ShellController : ControllerBase, IInitializableController
    {
        private readonly IPersistApplicationState statePersistence;
        private readonly UiContext uiContext;
        private Guid dialogCorrelationId;
        private bool doNotUseCancelButtonVisible;
        private bool doNotUseOkButtonVisible;

        private object doNotUsePopUpDialogContent;
        private string doNotUsePopUpTitle;
        private bool doNotUseSaveButtonVisible;
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
            MessengerInstance.Register<RequestShellDialogMessage>(this, OnPopUpDialogRequested);

            this.statePersistence = statePersistence;
            this.uiContext = uiContext;
            BackgroundJob = uiContext.BackgroundJob;
        }

        public IBackgroundProcessingJobMetadata BackgroundJob { get; private set; }

        public BudgetController BudgetController
        {
            get { return this.uiContext.BudgetController; }
        }

        public bool CancelButtonVisible
        {
            get { return this.doNotUseCancelButtonVisible; }
            set
            {
                this.doNotUseCancelButtonVisible = value;
                RaisePropertyChanged(() => CancelButtonVisible);
            }
        }

        public DashboardController DashboardController
        {
            get { return this.uiContext.DashboardController; }
        }

        public ICommand DialogCommand
        {
            get { return new RelayCommand<string>(OnDialogCommandExecute, CanDialogCommandExecute); }
        }

        public LedgerBookController LedgerBookController
        {
            get { return this.uiContext.LedgerBookController; }
        }

        public MainMenuController MainMenuController
        {
            get { return this.uiContext.MainMenuController; }
        }

        public RulesController RulesController
        {
            get { return this.uiContext.RulesController; }
        }

        public bool OkButtonVisible
        {
            get { return this.doNotUseOkButtonVisible; }
            set
            {
                this.doNotUseOkButtonVisible = value;
                RaisePropertyChanged(() => OkButtonVisible);
                RaisePropertyChanged(() => DialogOkIsCancel);
            }
        }

        public object PopUpDialogContent
        {
            get { return this.doNotUsePopUpDialogContent; }
            set
            {
                this.doNotUsePopUpDialogContent = value;
                RaisePropertyChanged(() => PopUpDialogContent);
            }
        }

        public string PopUpTitle
        {
            get { return this.doNotUsePopUpTitle; }
            set
            {
                this.doNotUsePopUpTitle = value;
                RaisePropertyChanged(() => PopUpTitle);
            }
        }

        public bool DialogOkIsCancel
        {
            get { return OkButtonVisible && !CancelButtonVisible && !SaveButtonVisible; }
        }

        public ReportsCatalogController ReportsCatalogController
        {
            get { return this.uiContext.ReportsCatalogController; }
        }

        public bool SaveButtonVisible
        {
            get { return this.doNotUseSaveButtonVisible; }
            set
            {
                this.doNotUseSaveButtonVisible = value;
                RaisePropertyChanged(() => SaveButtonVisible);
            }
        }

        public StatementController StatementController
        {
            get { return this.uiContext.StatementController; }
        }

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

            IEnumerable<IPersistent> rehydratedModels = this.statePersistence.Load();
            MessengerInstance.Send(new ApplicationStateLoadedMessage(rehydratedModels));
            MessengerInstance.Send(new ApplicationStateLoadFinishedMessage());

            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        public void OnViewReady()
        {
            // Re-run the initialisers. This allows any controller who couldn't initialise until the views are loaded to now reattempt to initialise.
            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        private bool CanDialogCommandExecute(string arg)
        {
            return PopUpDialogContent != null && !string.IsNullOrWhiteSpace(arg) && BackgroundJob.MenuAvailable;
        }

        private void OnDialogCommandExecute(string commandType)
        {
            // Delay execution so that keyed events happen
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                switch (commandType)
                {
                    case "Ok":
                    case "Save":
                        MessengerInstance.Send(new ShellDialogResponseMessage(PopUpDialogContent, ShellDialogResponse.Ok) { CorrelationId = this.dialogCorrelationId });
                        break;

                    case "Cancel":
                        MessengerInstance.Send(new ShellDialogResponseMessage(PopUpDialogContent, ShellDialogResponse.Cancel) { CorrelationId = this.dialogCorrelationId });
                        break;

                    default:
                        throw new NotSupportedException("Unsupported command type received from Dialog Popup on Shell view. " + commandType);
                }

                PopUpDialogContent = null;
            });
        }

        private void OnPopUpDialogRequested(RequestShellDialogMessage message)
        {
            PopUpTitle = message.Title;
            PopUpDialogContent = message.Content;
            OkButtonVisible = message.DialogType == ShellDialogType.Ok || message.DialogType == ShellDialogType.OkCancel;
            SaveButtonVisible = message.DialogType == ShellDialogType.SaveCancel;
            CancelButtonVisible = message.DialogType != ShellDialogType.Ok;
            this.dialogCorrelationId = message.CorrelationId;
        }

        private void OnShutdownRequested(ShutdownMessage message)
        {
            var gatherDataMessage = new ApplicationStateRequestedMessage();
            MessengerInstance.Send(gatherDataMessage);
            this.statePersistence.Persist(gatherDataMessage.PersistentData);
        }
    }
}