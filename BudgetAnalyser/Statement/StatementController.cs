using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser.Statement
{
    public class StatementController : ControllerBase, IShowableController, IInitializableController
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly IRecentFileManager recentFileManager;
        private readonly IStatementFileManager statementFileManager;
        private readonly UiContext uiContext;
        private bool doNotUseShown;
        private bool initialised;
        private List<ICommand> recentFileCommands;

        private string waitingForBudgetToLoad;

        public StatementController(
            [NotNull] UiContext uiContext,
            [NotNull] IStatementFileManager statementFileManager,
            [NotNull] IBudgetBucketRepository budgetBucketRepository,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] DemoFileHelper demoFileHelper)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statementFileManager == null)
            {
                throw new ArgumentNullException("statementFileManager");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            if (recentFileManager == null)
            {
                throw new ArgumentNullException("recentFileManager");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            ViewModel = new StatementViewModel(budgetBucketRepository);
            this.uiContext = uiContext;
            this.statementFileManager = statementFileManager;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;

            MessagingGate.Register<FilterAppliedMessage>(this, OnFilterApplied);
            MessagingGate.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessagingGate.Register<BudgetReadyMessage>(this, OnBudgetReadyMessage);
        }

        public AppliedRulesController AppliedRulesController
        {
            get { return this.uiContext.AppliedRulesController; }
        }

        // TODO Need a find feature to find and highlight transactions based on text search

        public IBackgroundProcessingJobMetadata BackgroundJob
        {
            get { return this.uiContext.BackgroundJob; }
        }

        public ICommand CloseStatementCommand
        {
            get { return new RelayCommand(OnCloseStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return new RelayCommand(OnDeleteTransactionCommandExecute, CanExecuteDeleteTransactionCommand); }
        }

        public ICommand DemoStatementCommand
        {
            get { return new RelayCommand(OnDemoStatementCommandExecuted, CanExecuteOpenStatementCommand); }
        }

        public ICommand MergeStatementCommand
        {
            get { return new RelayCommand(OnMergeStatementCommandExecute, CanExecuteCloseStatementCommand); }
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

        public ICommand SaveStatementCommand
        {
            get { return new RelayCommand(OnSaveStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public Transaction SelectedRow { get; set; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public StatementViewModel ViewModel { get; private set; }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;
            UpdateRecentFiles(this.recentFileManager.Files());
        }

        public void NotifyOfClosing()
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }
        }

        public void NotifyOfEdit()
        {
            ViewModel.Dirty = true;
            Messenger.Send(new StatementHasBeenModifiedMessage { Dirty = ViewModel.Dirty });
        }

        public void NotifyOfReset()
        {
            ViewModel.Dirty = false;
            Messenger.Send(new StatementHasBeenModifiedMessage { Dirty = false });
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && ViewModel.Statement != null;
        }

        private bool CanExecuteDeleteTransactionCommand()
        {
            return SelectedRow != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable;
        }

        private bool Load(string fullFileName)
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }

            try
            {
                BackgroundJob.StartNew("Loading statement...", false);
                return LoadInternal(fullFileName);
            }
            finally
            {
                BackgroundJob.Finish();
            }
        }

        private bool LoadInternal(string fullFileName)
        {
            StatementModel statementModel = this.statementFileManager.LoadAnyStatementFile(fullFileName);

            using (this.uiContext.WaitCursorFactory())
            {
                if (statementModel == null)
                {
                    // User cancelled.
                    return false;
                }

                ViewModel.Statement = statementModel;
                var requestCurrentFilterMessage = new RequestFilterMessage(this);
                Messenger.Send(requestCurrentFilterMessage);
                if (requestCurrentFilterMessage.Criteria != null)
                {
                    ViewModel.Statement.Filter(requestCurrentFilterMessage.Criteria);
                }

                NotifyOfReset();
                ViewModel.TriggerRefreshTotalsRow();
            }

            MessagingGate.Send(new StatementReadyMessage(ViewModel.Statement));
            return true;
        }

        private void LoadStatementFromApplicationState(string statementFileName)
        {
            try
            {
                BackgroundJob.StartNew("Loading previous accounts...", false);
                if (string.IsNullOrWhiteSpace(statementFileName))
                {
                    return;
                }

                if (ViewModel.BudgetModel == null)
                {
                    // Budget isn't yet loaded. Wait for the next BudgetClosedMessage to signal budget is ready.
                    this.waitingForBudgetToLoad = statementFileName;
                    return;
                }

                LoadInternal(statementFileName);
            }
            catch (FileNotFoundException)
            {
                // Ignore it.
            }
            finally
            {
                BackgroundJob.Finish();
            }
        }

        private void Merge()
        {
            Save();
            ViewModel.BucketFilter = null;

            try
            {
                BackgroundJob.StartNew("Merging statement...", false);
                StatementModel additionalModel = this.statementFileManager.ImportAndMergeBankStatement(ViewModel.Statement);
                using (this.uiContext.WaitCursorFactory())
                {
                    if (additionalModel == null)
                    {
                        // User cancelled.
                        return;
                    }

                    ViewModel.Statement.Merge(additionalModel);
                }

                RaisePropertyChanged(() => ViewModel);
                Messenger.Send(new TransactionsChangedMessage());
                NotifyOfEdit();
                ViewModel.TriggerRefreshTotalsRow();
            }
            finally
            {
                MessagingGate.Send(new StatementReadyMessage(ViewModel.Statement));
                BackgroundJob.Finish();
            }
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(LastStatementLoadedV1)))
            {
                return;
            }

            var statementFileName = message.RehydratedModels[typeof(LastStatementLoadedV1)].AdaptModel<string>();
            LoadStatementFromApplicationState(statementFileName);
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastStatement = new LastStatementLoadedV1
            {
                Model = ViewModel.Statement == null ? null : ViewModel.Statement.FileName,
            };
            message.PersistThisModel(lastStatement);
        }

        private void OnBudgetReadyMessage(BudgetReadyMessage message)
        {
            if (!message.ActiveBudget.BudgetActive)
            {
                // Not the current budget for today so ignore.
                return;
            }

            BudgetModel oldBudget = ViewModel.BudgetModel;
            ViewModel.BudgetModel = message.ActiveBudget.Model;

            if (this.waitingForBudgetToLoad != null)
            {
                // We've been waiting for the budget to load so we can load previous statement.
                LoadStatementFromApplicationState(this.waitingForBudgetToLoad);
                this.waitingForBudgetToLoad = null;
                return;
            }

            if (oldBudget != null
                && (oldBudget.Expenses.Any() || oldBudget.Incomes.Any())
                && oldBudget.Id != ViewModel.BudgetModel.Id
                && ViewModel.Statement != null
                && ViewModel.Statement.AllTransactions.Any())
            {
                this.uiContext.UserPrompts.MessageBox.Show(
                    "WARNING! By loading a different budget with a Statement loaded data loss may occur. There may be budget categories used in the Statement that do not exist in the loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                    "Data Loss Wanring!");
            }
        }

        private void OnCloseStatementExecute()
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }

            ViewModel.Statement = null;
            NotifyOfReset();
            ViewModel.TriggerRefreshTotalsRow();
        }

        private void OnDeleteTransactionCommandExecute()
        {
            if (SelectedRow == null)
            {
                return;
            }

            bool? confirm = this.uiContext.UserPrompts.YesNoBox.Show(
                "Are you sure you want to delete this transaction?", "Delete Transaction");
            if (confirm != null && confirm.Value)
            {
                ViewModel.Statement.RemoveTransaction(SelectedRow);
                ViewModel.TriggerRefreshTotalsRow();
                NotifyOfEdit();
            }
        }

        private void OnDemoStatementCommandExecuted()
        {
            OnOpenStatementExecute(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
        }

        private void OnFilterApplied(FilterAppliedMessage message)
        {
            if (message.Sender == this || message.Criteria == null)
            {
                return;
            }

            if (ViewModel.Statement == null)
            {
                return;
            }

            ViewModel.Statement.Filter(message.Criteria);
            ViewModel.TriggerRefreshTotalsRow();
        }

        private void OnMergeStatementCommandExecute()
        {
            Merge();
        }

        private void OnOpenStatementExecute(string fullFileName)
        {
            try
            {
                if (!Load(fullFileName))
                {
                    return;
                }

                UpdateRecentFiles(this.recentFileManager.AddFile(ViewModel.Statement.FileName));
            }
            catch (FileNotFoundException ex)
            {
                // When merging this exception will never be thrown.
                if (!string.IsNullOrWhiteSpace(ex.FileName))
                {
                    UpdateRecentFiles(this.recentFileManager.Remove(ex.FileName));
                }
            }
        }

        private void OnSaveStatementExecute()
        {
            // Not async at this stage, because saving of data while user edits are taking place will result in inconsistent results.
            using (this.uiContext.WaitCursorFactory())
            {
                Save();
                UpdateRecentFiles(this.recentFileManager.UpdateFile(ViewModel.Statement.FileName));
            }
        }

        private bool PromptToSaveIfDirty()
        {
            if (ViewModel.Statement != null && ViewModel.Dirty)
            {
                bool? result = this.uiContext.UserPrompts.YesNoBox.Show("Statement has been modified, save changes?",
                    "Budget Analyser");
                if (result != null && result.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void Save()
        {
            this.statementFileManager.Save(ViewModel.Statement);
            ViewModel.TriggerRefreshTotalsRow();
            NotifyOfReset();
        }

        private void UpdateRecentFiles(IEnumerable<KeyValuePair<string, string>> files)
        {
            this.recentFileCommands =
                files.Select(f => (ICommand)new RecentFileRelayCommand(f.Value, f.Key, OnOpenStatementExecute, x => BackgroundJob.MenuAvailable))
                    .ToList();
            RaisePropertyChanged(() => RecentFile1Command);
            RaisePropertyChanged(() => RecentFile2Command);
            RaisePropertyChanged(() => RecentFile3Command);
            RaisePropertyChanged(() => RecentFile4Command);
            RaisePropertyChanged(() => RecentFile5Command);
        }
    }
}