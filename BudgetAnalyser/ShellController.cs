using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.SpendingTrend;
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
        private GlobalFilterCriteria doNotUseGlobalFilterCriteria;
        private bool doNotUseTabMenuShown;
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

            MessagingGate.Register<BudgetReadyMessage>(this, OnBudgetReady);
            MessagingGate.Register<StatementHasBeenModifiedMessage>(this, OnStatementModified);
            MessagingGate.Register<ShutdownMessage>(this, OnShutdownRequested);
            MessagingGate.Register<FilterAppliedMessage>(this, OnFilterAppliedMessageReceived);
            MessagingGate.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessagingGate.Register<RequestFilterMessage>(this, OnGlobalFilterRequested);

            this.recentFileManager = recentFileManager;
            this.statePersistence = statePersistence;
            this.uiContext = uiContext;
            BackgroundJob = uiContext.BackgroundJob;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
        }

        public ICommand AnalyseStatementCommand
        {
            get { return new RelayCommand(OnAnalyseStatementCommandExecute, CanExecuteAnalyseStatementCommand); }
        }

        public IBackgroundProcessingJobMetadata BackgroundJob { get; private set; }

        public BudgetController BudgetController
        {
            get { return this.uiContext.BudgetController; }
        }

        public BudgetPieController BudgetPieController
        {
            get { return this.uiContext.BudgetPieController; }
        }

        public ICommand BudgetSelectionCommand
        {
            get { return new RelayCommand(OnBudgetSelectionCommandExecute, CanExecuteBudgetSelectionCommand); }
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

        public ICommand EditBudgetCommand
        {
            get { return new RelayCommand(OnEditBudgetExecute, CanExecuteEditBudgetCommand); }
        }

        public ICommand GlobalAccountTypeFilterCommand
        {
            get { return new RelayCommand<string>(OnGlobalDateFilterCommandExecute, CanExecuteGlobalFilterCommand); }
        }

        public bool GlobalAccountTypeFilterEnabled
        {
            get
            {
                return GlobalFilterCriteria != null
                       && !GlobalFilterCriteria.Cleared
                       && GlobalFilterCriteria.AccountType != null;
            }

            set { }
        }

        public ICommand GlobalDateFilterCommand
        {
            get { return new RelayCommand<string>(OnGlobalDateFilterCommandExecute, CanExecuteGlobalFilterCommand); }
        }

        public bool GlobalDateFilterEnabled
        {
            get
            {
                return GlobalFilterCriteria != null
                       && !GlobalFilterCriteria.Cleared
                       && GlobalFilterCriteria.BeginDate != null
                       && GlobalFilterCriteria.EndDate != null;
            }

            set { }
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

        public ICommand SpendingTrendCommand
        {
            get { return new RelayCommand(OnSpendingTrendCommandExecute, CanExecuteAnalyseStatementCommand); }
        }

        public SpendingTrendController SpendingTrendController
        {
            get { return this.uiContext.SpendingTrendController; }
        }

        public StatementController StatementController
        {
            get { return this.uiContext.StatementController; }
        }

        public bool TabMenuShown
        {
            get { return this.doNotUseTabMenuShown; }
            set
            {
                this.doNotUseTabMenuShown = value;
                RaisePropertyChanged(() => TabMenuShown);
            }
        }

        public ICommand ViewBudgetPieCommand
        {
            get { return new RelayCommand(OnViewBudgetPieCommandExecute, CanExecuteViewBudgetPieCommand); }
        }

        public ICommand ViewLedgerBookCommand
        {
            get { return new RelayCommand(OnViewLedgerBookCommandExecute, CanExecuteViewLedgerBookCommand); }
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

        private GlobalFilterCriteria GlobalFilterCriteria
        {
            get { return this.doNotUseGlobalFilterCriteria; }

            set
            {
                this.doNotUseGlobalFilterCriteria = value;
                RaisePropertyChanged(() => GlobalFilterCriteria);
                RaisePropertyChanged(() => GlobalDateFilterEnabled);
            }
        }

        public void Initialize()
        {
            StatementController.Initialize();

            IEnumerable<IPersistent> rehydratedModels = this.statePersistence.Load();
            Messenger.Send(new ApplicationStateLoadedMessage(rehydratedModels));
            UpdateRecentFiles(this.recentFileManager.Files());
        }

        private bool AnyControllersShown(ControllerBase exceptThisOne)
        {
            return this.uiContext.ShowableControllers.Where(c => c.GetType() != exceptThisOne.GetType()).Any(c => c.Shown);
        }

        private bool CanExecuteAnalyseStatementCommand()
        {
            return BackgroundJob.MenuAvailable
                   && !AnyControllersShown(StatementController)
                   && StatementController.Statement != null
                   && StatementController.Statement.Transactions.Any()
                   && StatementController.BudgetModel != null;
        }

        private bool CanExecuteBudgetSelectionCommand()
        {
            return BackgroundJob.MenuAvailable && !AnyControllersShown(BudgetController);
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && StatementController.Statement != null;
        }

        private bool CanExecuteEditBudgetCommand()
        {
            return BackgroundJob.MenuAvailable && !AnyControllersShown(BudgetController);
        }

        private bool CanExecuteGlobalFilterCommand(string parameter)
        {
            return BackgroundJob.MenuAvailable
                   && !BudgetController.Shown
                   && StatementController.Statement != null
                   && StatementController.BudgetModel != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable && !AnyControllersShown(StatementController);
        }

        private bool CanExecuteViewBudgetPieCommand()
        {
            if (!BackgroundJob.MenuAvailable || AnyControllersShown(BudgetPieController))
            {
                return false;
            }

            if (BudgetController.Expenses == null || BudgetController.Incomes == null || StatementController.BudgetModel == null)
            {
                return false;
            }

            return BudgetController.Expenses.Any() || BudgetController.Incomes.Any();
        }

        private bool CanExecuteViewLedgerBookCommand()
        {
            return !AnyControllersShown(LedgerBookController) && BackgroundJob.MenuAvailable;
        }

        private void OnAnalyseStatementCommandExecute()
        {
            OverallPerformanceBudgetAnalysis analysis;
            using (this.uiContext.WaitCursorFactory())
            {
                analysis = this.uiContext.AnalysisFactory.Analyse(StatementController.Statement, BudgetController.Budgets, GlobalFilterCriteria);
            }

            this.uiContext.AnalysisFactory.ShowDialog(analysis);
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(PersistentFiltersV1)))
            {
                return;
            }

            var rehydratedFilters = message.RehydratedModels[typeof(PersistentFiltersV1)].AdaptModel<FilterStateV1>();
            GlobalFilterCriteria = new GlobalFilterCriteria
            {
                AccountType = rehydratedFilters.AccountType,
                BeginDate = rehydratedFilters.BeginDate,
                EndDate = rehydratedFilters.EndDate,
            };

            SendFilterAppliedMessage();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            bool noCriteria = GlobalFilterCriteria == null;
            var filterState = new PersistentFiltersV1
            {
                Model = new FilterStateV1
                {
                    BeginDate = noCriteria ? null : GlobalFilterCriteria.BeginDate,
                    EndDate = noCriteria ? null : GlobalFilterCriteria.EndDate,
                    AccountType = noCriteria ? null : GlobalFilterCriteria.AccountType,
                },
            };

            message.PersistThisModel(filterState);
        }

        private void OnBudgetReady(BudgetReadyMessage message)
        {
            if (message.Budget.BudgetActive)
            {
                StatementController.BudgetModel = message.Budget.Model;
            }

            BudgetController.Shown = false;
        }

        private void OnBudgetSelectionCommandExecute()
        {
            BudgetController.SelectOtherBudget();
        }

        private void OnCloseStatementExecute()
        {
            StatementController.CloseStatement();
        }

        private void OnEditBudgetExecute()
        {
            if (BudgetController.Shown)
            {
                BudgetPieController.Close();
                if (!BudgetController.ValidateAndClose())
                {
                    return;
                }

                return;
            }

            BudgetController.Shown = !BudgetController.Shown;
        }

        private void OnFilterAppliedMessageReceived(FilterAppliedMessage message)
        {
            if (message.Sender is StatementController)
            {
                GlobalFilterCriteria = message.Criteria;
            }

            RaisePropertyChanged("GlobalDateFilterEnabled");
            RaisePropertyChanged("GlobalAccountTypeFilterEnabled");
        }

        private void OnGlobalDateFilterCommandExecute(string filterType)
        {
            this.uiContext.GlobalFilterController.InitialValues(GlobalFilterCriteria);
            if (filterType == "Date")
            {
                this.uiContext.GlobalFilterController.PromptUserForDates();
            }
            else if (filterType == "AccountType")
            {
                this.uiContext.GlobalFilterController.PromptUserForAccountType(StatementController.Statement.AccountTypes);
            }

            GlobalFilterCriteria = this.uiContext.GlobalFilterController.Criteria;
            SendFilterAppliedMessage();
        }

        private void OnGlobalFilterRequested(RequestFilterMessage message)
        {
            message.Criteria = GlobalFilterCriteria;
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

        private void OnSpendingTrendCommandExecute()
        {
            using (this.uiContext.WaitCursorFactory())
            {
                SpendingTrendController.Load(StatementController.Statement, StatementController.BudgetModel, GlobalFilterCriteria);
            }
        }

        private void OnStatementModified(StatementHasBeenModifiedMessage message)
        {
            this.dirtyFlag = message.Dirty ? "*" : string.Empty;
            RaisePropertyChanged(() => WindowTitle);
        }

        private void OnViewBudgetPieCommandExecute()
        {
            BudgetPieController.Load(BudgetController.CurrentBudget.Model);
        }

        private void OnViewLedgerBookCommandExecute()
        {
            LedgerBookController.Show(StatementController.Statement, BudgetController.CurrentBudget);
        }

        private void SendFilterAppliedMessage()
        {
            BackgroundJob.StartNew("Filtering transactions...", false);
            Messenger.Send(new FilterAppliedMessage(this, GlobalFilterCriteria));
            BackgroundJob.Finish();
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