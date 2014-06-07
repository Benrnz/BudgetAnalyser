using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser.Statement
{
    public class StatementControllerFileOperations : ViewModelBase
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly IRecentFileManager recentFileManager;
        private readonly IStatementFileManager statementFileManager;
        private readonly IUiContext uiContext;
        private Dispatcher dispatcher;
        private List<ICommand> recentFileCommands;

        public StatementControllerFileOperations(
            [NotNull] IUiContext uiContext,
            [NotNull] IStatementFileManager statementFileManager,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] DemoFileHelper demoFileHelper,
            [NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statementFileManager == null)
            {
                throw new ArgumentNullException("statementFileManager");
            }

            if (recentFileManager == null)
            {
                throw new ArgumentNullException("recentFileManager");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.uiContext = uiContext;
            this.statementFileManager = statementFileManager;
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
            ViewModel = new StatementViewModel(budgetBucketRepository);
        }

        public ICommand CloseStatementCommand
        {
            get { return new RelayCommand(OnCloseStatementExecute, CanExecuteCloseStatementCommand); }
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

        internal StatementViewModel ViewModel { get; private set; }
        internal string WaitingForBudgetToLoad { get; private set; }

        private IBackgroundProcessingJobMetadata BackgroundJob
        {
            get { return this.uiContext.BackgroundJob; }
        }

        public void NotifyOfClosing()
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }
        }

        internal void Initialise(Dispatcher currentDispatcher, StatementController controller)
        {
            this.dispatcher = currentDispatcher;
            ViewModel.Initialise(controller);
        }

        internal async void LoadStatementFromApplicationState(string statementFileName)
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
                    WaitingForBudgetToLoad = statementFileName;
                    return;
                }

                await LoadInternal(statementFileName);
                WaitingForBudgetToLoad = null;
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

        internal void NotifyOfEdit()
        {
            ViewModel.Dirty = true;
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(ViewModel.Dirty, ViewModel.Statement));
        }

        internal void UpdateRecentFiles()
        {
            UpdateRecentFiles(this.recentFileManager.Files());
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && ViewModel.Statement != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable;
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

            await this.dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
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

        private void NotifyOfReset()
        {
            ViewModel.Dirty = false;
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(false, ViewModel.Statement));
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

        private void OnDemoStatementCommandExecuted()
        {
            OnOpenStatementExecute(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
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
                    if (additionalModel == null)
                    {
                        // User cancelled.
                        return;
                    }

                    this.dispatcher.BeginInvoke(DispatcherPriority.Normal, () => cursor = this.uiContext.WaitCursorFactory());
                    ViewModel.Statement.Merge(additionalModel);

                    this.dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
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
                        this.dispatcher.BeginInvoke(DispatcherPriority.Normal, () => cursor.Dispose());
                    }

                    BackgroundJob.Finish();
                    if (additionalModel != null)
                    {
                        this.dispatcher.BeginInvoke(DispatcherPriority.Normal, () => MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement)));
                    }
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