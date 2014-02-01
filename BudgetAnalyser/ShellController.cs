using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ShellController : ControllerBase, IInitializableController
    {
        private readonly IRecentFileManager recentFileManager;
        private readonly IPersistApplicationState statePersistence;
        private readonly UiContext uiContext;

        private string dirtyFlag = string.Empty;
        private bool initialised;
        private List<ICommand> recentFileCommands;

        // TODO Upgrade all windows to be win8 style inline content, and not a separate window.
        public ShellController(
            [NotNull] UiContext uiContext,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] IPersistApplicationState statePersistence)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (recentFileManager == null)
            {
                throw new ArgumentNullException("recentFileManager");
            }

            if (statePersistence == null)
            {
                throw new ArgumentNullException("statePersistence");
            }

            MessagingGate.Register<StatementHasBeenModifiedMessage>(this, OnStatementModified);
            MessagingGate.Register<ShutdownMessage>(this, OnShutdownRequested);

            this.recentFileManager = recentFileManager;
            this.statePersistence = statePersistence;
            this.uiContext = uiContext;
            BackgroundJob = uiContext.BackgroundJob;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
        }

        public IBackgroundProcessingJobMetadata BackgroundJob { get; private set; }

        public BudgetController BudgetController
        {
            get { return this.uiContext.BudgetController; }
        }

        // TODO Would like to make all commands execute background tasks and use async.
        public ICommand CloseStatementCommand
        {
            get { return new RelayCommand(OnCloseStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public DashboardController DashboardController
        {
            get { return this.uiContext.DashboardController; }
        }

        public LedgerBookController LedgerBookController
        {
            get { return this.uiContext.LedgerBookController; }
        }

        public MainMenuController MainMenuController
        {
            get { return this.uiContext.MainMenuController; }
        }

        public ICommand MergeStatementCommand
        {
            get { return new RelayCommand(() => OnMergeStatementCommandExecute(), CanExecuteCloseStatementCommand); }
        }

        public ICommand OpenStatementCommand
        {
            get { return new RelayCommand(() => OnOpenStatementExecute(null), CanExecuteOpenStatementCommand); }
        }

        public ICommand RecentFile1Command
        {
            get
            {
                if (this.recentFileCommands.Count > 0)
                {
                    return this.recentFileCommands[0];
                }

                return null;
            }
        }

        public ICommand RecentFile2Command
        {
            get
            {
                if (this.recentFileCommands.Count > 1)
                {
                    return this.recentFileCommands[1];
                }

                return null;
            }
        }

        public ICommand RecentFile3Command
        {
            get
            {
                if (this.recentFileCommands.Count > 2)
                {
                    return this.recentFileCommands[2];
                }

                return null;
            }
        }

        public ICommand RecentFile4Command
        {
            get
            {
                if (this.recentFileCommands.Count > 3)
                {
                    return this.recentFileCommands[3];
                }

                return null;
            }
        }

        public ICommand RecentFile5Command
        {
            get
            {
                if (this.recentFileCommands.Count > 4)
                {
                    return this.recentFileCommands[4];
                }

                return null;
            }
        }

        public ReportsCatalogController ReportsCatalogController
        {
            get { return this.uiContext.ReportsCatalogController; }
        }

        public ICommand SaveStatementCommand
        {
            get { return new RelayCommand(OnSaveStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public StatementController StatementController
        {
            get { return this.uiContext.StatementController; }
        }

        public string WindowTitle
        {
            get
            {
                string fileName = StatementController.Statement == null
                    ? string.Empty
                    : StatementController.Statement.FileName;
                return string.Format("{0} Budget Analyser {1}", this.dirtyFlag, fileName);
            }
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;

            IEnumerable<IPersistent> rehydratedModels = this.statePersistence.Load();
            Messenger.Send(new ApplicationStateLoadedMessage(rehydratedModels));
            UpdateRecentFiles(this.recentFileManager.Files());

            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        public void OnViewReady()
        {
            // Re-run the initialisers. This allows any controller who couldn't initialise until the views are loaded to now reattempt to initialise.
            this.uiContext.Controllers.OfType<IInitializableController>().ToList().ForEach(i => i.Initialize());
        }

        private bool AnyControllersShown(ControllerBase exceptThisOne)
        {
            return this.uiContext.ShowableControllers.Where(c => c.GetType() != exceptThisOne.GetType()).Any(c => c.Shown);
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && StatementController.Statement != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable && !AnyControllersShown(StatementController);
        }

        private void OnCloseStatementExecute()
        {
            StatementController.CloseStatement();
        }

        private void OnMergeStatementCommandExecute()
        {
            BudgetController.Shown = false;
            StatementController.Merge();
        }

        private void OnOpenStatementExecute(string fullFileName)
        {
            BudgetController.Shown = false;
            try
            {
                if (!StatementController.Load(fullFileName))
                {
                    return;
                }

                UpdateRecentFiles(this.recentFileManager.AddFile(StatementController.Statement.FileName));
            }
            catch (FileNotFoundException ex)
            {
                // When merging this exception will never be thrown.
                if (!string.IsNullOrWhiteSpace(ex.FileName))
                {
                    UpdateRecentFiles(this.recentFileManager.Remove(ex.FileName));
                }
            }

            RaisePropertyChanged(() => WindowTitle);
        }

        private void OnSaveStatementExecute()
        {
            // Not async at this stage, because saving of data while user edits are taking place will result in inconsistent results.
            using (this.uiContext.WaitCursorFactory())
            {
                StatementController.Save();
                UpdateRecentFiles(this.recentFileManager.UpdateFile(StatementController.Statement.FileName));
            }
        }

        private void OnShutdownRequested(ShutdownMessage message)
        {
            var gatherDataMessage = new ApplicationStateRequestedMessage();
            Messenger.Send(gatherDataMessage);
            this.statePersistence.Persist(gatherDataMessage.PersistentData);
        }

        private void OnStatementModified(StatementHasBeenModifiedMessage message)
        {
            this.dirtyFlag = message.Dirty ? "*" : string.Empty;
            RaisePropertyChanged(() => WindowTitle);
        }

        private void UpdateRecentFiles(IEnumerable<KeyValuePair<string, string>> files)
        {
            this.recentFileCommands =
                files.Select(f => (ICommand)new RecentFileRelayCommand(f.Value, f.Key, file => OnOpenStatementExecute(file), x => BackgroundJob.MenuAvailable))
                    .ToList();
            RaisePropertyChanged(() => RecentFile1Command);
            RaisePropertyChanged(() => RecentFile2Command);
            RaisePropertyChanged(() => RecentFile3Command);
            RaisePropertyChanged(() => RecentFile4Command);
            RaisePropertyChanged(() => RecentFile5Command);
        }
    }
}