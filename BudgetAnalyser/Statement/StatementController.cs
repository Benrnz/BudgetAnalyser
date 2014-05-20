using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser.Statement
{
    public class StatementController : ControllerBase, IShowableController, IInitializableController
    {
        public const string SortByBucketKey = "Bucket";
        public const string SortByDateKey = "Date";

        private readonly DemoFileHelper demoFileHelper;
        private readonly IRecentFileManager recentFileManager;
        private readonly IStatementFileManager statementFileManager;
        private readonly IUiContext uiContext;
        private Transaction doNotUseSelectedRow;
        private bool doNotUseShown;
        private bool initialised;
        private List<ICommand> recentFileCommands;
        private Guid shellDialogCorrelationId;

        private string waitingForBudgetToLoad;

        public StatementController(
            [NotNull] IUiContext uiContext,
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

            ViewModel = new StatementViewModel(this, budgetBucketRepository);
            this.uiContext = uiContext;
            this.statementFileManager = statementFileManager;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<FilterAppliedMessage>(this, OnFilterApplied);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseMessageReceived);
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
            get { return new RelayCommand(OnDeleteTransactionCommandExecute, HasSelectedRow); }
        }

        public ICommand DemoStatementCommand
        {
            get { return new RelayCommand(OnDemoStatementCommandExecuted, CanExecuteOpenStatementCommand); }
        }

        public ICommand EditTransactionCommand
        {
            get { return new RelayCommand(OnEditTransactionCommandExecute, HasSelectedRow); }
        }

        public EditingTransactionController EditingTransactionController
        {
            get { return this.uiContext.EditingTransactionController; }
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

        public Transaction SelectedRow
        {
            get { return this.doNotUseSelectedRow; }
            set
            {
                this.doNotUseSelectedRow = value;
                RaisePropertyChanged(() => SelectedRow);
            }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown)
                {
                    return;
                }
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public ICommand SortCommand
        {
            get { return new RelayCommand(OnSortCommandExecute, CanExecuteSortCommand); }
        }

        public ICommand SplitTransactionCommand
        {
            get { return new RelayCommand(OnSplitTransactionCommandExecute, HasSelectedRow); }
        }

        public SplitTransactionController SplitTransactionController
        {
            get { return this.uiContext.SplitTransactionController; }
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
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(ViewModel.Dirty, ViewModel.Statement));
        }

        public void NotifyOfReset()
        {
            ViewModel.Dirty = false;
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(false, ViewModel.Statement));
        }

        public void RegisterListener<T>(object listener, Action<T> handler)
        {
            MessengerInstance.Register(listener, handler);
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && ViewModel.Statement != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable;
        }

        private bool CanExecuteSortCommand()
        {
            return BackgroundJob.MenuAvailable && ViewModel.Statement != null && ViewModel.Statement.Transactions.Any();
        }

        private void DeleteTransaction()
        {
            ViewModel.Statement.RemoveTransaction(SelectedRow);
            ViewModel.TriggerRefreshTotalsRow();
            NotifyOfEdit();
        }

        private void FinaliseEditTransaction(ShellDialogResponseMessage message)
        {
            if (message.Response == ShellDialogButton.Ok)
            {
                var viewModel = (EditingTransactionController)message.Content;
                if (viewModel.HasChanged)
                {
                    NotifyOfEdit();
                }
            }
        }

        private void FinaliseSplitTransaction(ShellDialogResponseMessage message)
        {
            if (message.Response == ShellDialogButton.Save)
            {
                ViewModel.Statement.SplitTransaction(
                    SplitTransactionController.OriginalTransaction,
                    SplitTransactionController.SplinterAmount1,
                    SplitTransactionController.SplinterAmount2,
                    SplitTransactionController.SplinterBucket1,
                    SplitTransactionController.SplinterBucket2);

                ViewModel.TriggerRefreshTotalsRow();
                NotifyOfEdit();
            }
        }

        private bool HasSelectedRow()
        {
            return SelectedRow != null;
        }

        private async Task<bool> LoadInternal(string fullFileName)
        {
            StatementModel statementModel = null;
            await Task.Run(() => statementModel = this.statementFileManager.LoadAnyStatementFile(fullFileName));

            if (statementModel == null)
            {
                // User cancelled.
                return false;
            }

            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                // Update all UI bound properties.
                ViewModel.Statement = statementModel;
                var requestCurrentFilterMessage = new RequestFilterMessage(this);
                MessengerInstance.Send(requestCurrentFilterMessage);
                if (requestCurrentFilterMessage.Criteria != null)
                {
                    ViewModel.Statement.Filter(requestCurrentFilterMessage.Criteria);
                }

                NotifyOfReset();
                ViewModel.TriggerRefreshTotalsRow();

                MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement));
            });
            return true;
        }

        private async void LoadStatementFromApplicationState(string statementFileName)
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

                await LoadInternal(statementFileName);
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

        private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
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
                && oldBudget.Name != ViewModel.BudgetModel.Name
                && ViewModel.Statement != null
                && ViewModel.Statement.AllTransactions.Any())
            {
                this.uiContext.UserPrompts.MessageBox.Show(
                    "WARNING! By loading a different budget with a Statement loaded data loss may occur. There may be budget categories used in the Statement that do not exist in the loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                    "Data Loss Wanring!");
            }

            ViewModel.TriggerRefreshBucketFilterList();
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
            MessengerInstance.Send(new StatementReadyMessage(null));
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
                DeleteTransaction();
            }
        }

        private void OnDemoStatementCommandExecuted()
        {
            OnOpenStatementExecute(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
        }

        private void OnEditTransactionCommandExecute()
        {
            if (SelectedRow == null || this.shellDialogCorrelationId != Guid.Empty)
            {
                return;
            }

            this.shellDialogCorrelationId = Guid.NewGuid();
            EditingTransactionController.ShowDialog(SelectedRow, this.shellDialogCorrelationId);
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
            ViewModel.TriggerRefreshBucketFilter();
        }

        private void OnMergeStatementCommandExecute()
        {
            Save();
            ViewModel.BucketFilter = null;

            BackgroundJob.StartNew("Merging statement...", false);
            StatementModel additionalModel = null;
            Task<StatementModel> loadModelTask = Task.Factory.StartNew(() => additionalModel = this.statementFileManager.ImportAndMergeBankStatement(ViewModel.Statement));

            IWaitCursor cursor = null;
            loadModelTask.ContinueWith(t =>
            {
                try
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => cursor = this.uiContext.WaitCursorFactory());
                    if (additionalModel == null)
                    {
                        // User cancelled.
                        return;
                    }

                    ViewModel.Statement.Merge(additionalModel);

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                    {
                        RaisePropertyChanged(() => ViewModel);
                        MessengerInstance.Send(new TransactionsChangedMessage());
                        NotifyOfEdit();
                        ViewModel.TriggerRefreshTotalsRow();
                    });
                }
                finally
                {
                    if (cursor != null)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => cursor.Dispose());
                    }

                    BackgroundJob.Finish();
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement)));
                }
            });
        }

        private void OnOpenStatementExecute(string fullFileName)
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }

            BackgroundJob.StartNew("Loading statement...", false);

            // Will prompt for file name if its null, which it will be for clicking the Load button, but RecentFilesButtons also use this method which will have a filename.
            Task<bool> task = LoadInternal(fullFileName);
            task.ConfigureAwait(false);

            // When this task is complete the statement will be loaded successfully, or it will have failed. The Task<bool> Result contains this indicator.
            task.ContinueWith(t =>
            {
                BackgroundJob.Finish();
                if (!t.IsFaulted && t.Result)
                {
                    // Update RecentFile list for successfully loaded files only. 
                    UpdateRecentFiles(this.recentFileManager.AddFile(ViewModel.Statement.FileName));
                }

                if (t.IsFaulted && t.Exception != null)
                {
                    var fileNotFoundException = t.Exception.InnerExceptions.FirstOrDefault(e => e is FileNotFoundException) as FileNotFoundException;
                    if (fileNotFoundException != null)
                    {
                        if (!string.IsNullOrWhiteSpace(fileNotFoundException.FileName))
                        {
                            // Remove the bad file that caused the exception from the RecentFiles list.
                            UpdateRecentFiles(this.recentFileManager.Remove(fileNotFoundException.FileName));
                        }
                    }
                }
            });
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

        private void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
        {
            if (message.CorrelationId != Guid.Empty && message.CorrelationId == this.shellDialogCorrelationId)
            {
                if (message.Content is EditingTransactionController)
                {
                    FinaliseEditTransaction(message);
                }
                else if (message.Content is SplitTransactionController)
                {
                    FinaliseSplitTransaction(message);
                }

                this.shellDialogCorrelationId = Guid.Empty;
            }
        }

        private void OnSortCommandExecute()
        {
            // The bindings are processed before commands, so the bound boolean for SortByBucket will be set to true by now.
            ViewModel.UpdateGroupedByBucket();
        }

        private void OnSplitTransactionCommandExecute()
        {
            this.shellDialogCorrelationId = Guid.NewGuid();
            SplitTransactionController.ShowDialog(SelectedRow, this.shellDialogCorrelationId);
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