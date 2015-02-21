using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class StatementControllerFileOperations : ViewModelBase
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly LoadFileController loadFileController;
        private readonly IUserMessageBox messageBox;
        private readonly IRecentFileManager recentFileManager;
        private readonly IUserQuestionBoxYesNo yesNoBox;
        private bool doNotUseLoadingData;
        // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //private List<ICommand> recentFileCommands;
        private ITransactionManagerService transactionService;

        public StatementControllerFileOperations(
            [NotNull] IUiContext uiContext,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] DemoFileHelper demoFileHelper,
            [NotNull] LoadFileController loadFileController)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (recentFileManager == null)
            {
                throw new ArgumentNullException("recentFileManager");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            if (loadFileController == null)
            {
                throw new ArgumentNullException("loadFileController");
            }

            this.yesNoBox = uiContext.UserPrompts.YesNoBox;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;
            this.loadFileController = loadFileController;
            ViewModel = new StatementViewModel(uiContext);
            this.recentFileManager.StateDataRestored += OnRecentFileManagerStateRestored;
        }

        public bool LoadingData
        {
            get { return this.doNotUseLoadingData; }
            private set
            {
                this.doNotUseLoadingData = value;
                RaisePropertyChanged(() => LoadingData);
            }
        }

        public ICommand SaveStatementCommand
        {
            get { return new RelayCommand(OnSaveStatementExecute, CanExecuteCloseStatementCommand); }
        }

        internal StatementViewModel ViewModel { get; private set; }

        public async void NotifyOfClosingAsync()
        {
            if (PromptToSaveIfDirty())
            {
                await SaveAsync(true);
            }
        }

        internal bool CanExecuteCloseStatementCommand()
        {
            return ViewModel.Statement != null;
        }

        internal void Initialise(ITransactionManagerService transactionManagerService)
        {
            this.transactionService = transactionManagerService;
            ViewModel.Initialise(this.transactionService);
        }

        internal async Task<bool> LoadFileAsync(string fullFileName)
        {
            if (string.IsNullOrWhiteSpace(fullFileName))
            {
                fullFileName = await GetFileNameFromUser(StatementOpenMode.Open);
                if (string.IsNullOrWhiteSpace(fullFileName))
                {
                    // User cancelled
                    return false;
                }
            }

            StatementModel statementModel;
            try
            {
                statementModel = await this.transactionService.LoadStatementModelAsync(fullFileName);
            }
            catch (KeyNotFoundException ex)
            {
                FileCannotBeLoaded(ex);
                return false;
            }
            catch (StatementModelChecksumException ex)
            {
                this.messageBox.Show("The file being loaded is corrupt. The internal checksum does not match the transactions.\nFile Checksum:" + ex.FileChecksum);
                return false;
            }
            catch (DataFormatException ex)
            {
                FileCannotBeLoaded(ex);
                return false;
            }
            catch (NotSupportedException ex)
            {
                FileCannotBeLoaded(ex);
                return false;
            }
            finally
            {
                this.loadFileController.Reset();
            }

            LoadingData = true;

            await Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                () =>
                {
                    // Update all UI bound properties.
                    var requestCurrentFilterMessage = new RequestFilterMessage(this);
                    MessengerInstance.Send(requestCurrentFilterMessage);
                    if (requestCurrentFilterMessage.Criteria != null)
                    {
                        this.transactionService.FilterTransactions(requestCurrentFilterMessage.Criteria);
                    }

                    ViewModel.Statement = statementModel;
                    NotifyOfReset();
                    ViewModel.TriggerRefreshTotalsRow();

                    MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement));

                    LoadingData = false;
                });

            return true;
        }

        internal void Close()
        {
            ViewModel.Statement = null;
            NotifyOfReset();
            ViewModel.TriggerRefreshTotalsRow();
            MessengerInstance.Send(new StatementReadyMessage(null));
        }

        internal async Task MergeInNewTransactions()
        {
            await SaveAsync(false);

            string fileName = await GetFileNameFromUser(StatementOpenMode.Merge);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled
                return;
            }

            try
            {
                AccountType account = this.loadFileController.SelectedExistingAccountName;
                this.transactionService.ImportAndMergeBankStatement(fileName, account);

                RaisePropertyChanged(() => ViewModel);
                MessengerInstance.Send(new TransactionsChangedMessage());
                NotifyOfEdit();
                ViewModel.TriggerRefreshTotalsRow();
                MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement));
            }
            catch (NotSupportedException ex)
            {
                FileCannotBeLoaded(ex);
            }
            catch (KeyNotFoundException ex)
            {
                FileCannotBeLoaded(ex);
            }
            finally
            {
                this.loadFileController.Reset();
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

        private bool CanExecuteOpenStatementCommand()
        {
            return !LoadingData;
        }

        private bool CanExecuteRecentFileOpenCommand(string parameter)
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            return CanExecuteOpenStatementCommand() && !string.IsNullOrWhiteSpace(parameter);
        }

        private void FileCannotBeLoaded(Exception ex)
        {
            this.messageBox.Show("The file cannot be loaded.\n" + ex.Message);
        }

        /// <summary>
        ///     Prompts the user for a filename and other required parameters to be able to load/import/merge the file.
        /// </summary>
        /// <param name="mode">Open or Merge mode.</param>
        /// <returns>
        ///     The user selected filename. All other required parameters are accessible from the
        ///     <see cref="LoadFileController" />.
        /// </returns>
        private async Task<string> GetFileNameFromUser(StatementOpenMode mode)
        {
            switch (mode)
            {
                case StatementOpenMode.Merge:
                    await this.loadFileController.RequestUserInputForMerging(ViewModel.Statement);
                    break;

                case StatementOpenMode.Open:
                    await this.loadFileController.RequestUserInputForOpenFile();
                    break;
            }

            return this.loadFileController.FileName;
        }

        private void NotifyOfReset()
        {
            ViewModel.Dirty = false;
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(false, ViewModel.Statement));
        }

        private void OnDemoStatementCommandExecuted()
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            OnOpenStatementExecuteAsync(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
        }

        private async void OnOpenStatementExecuteAsync(string fullFileName)
        {
            if (PromptToSaveIfDirty())
            {
                await SaveAsync(true);
            }

            // Will prompt for file name if its null, which it will be for clicking the Load button, but RecentFilesButtons also use this method which will have a filename.
            try
            {
                bool result = await LoadFileAsync(fullFileName);

                // When this task is complete the statement will be loaded successfully, or it will have failed. The Task<bool> Result contains this indicator.
                if (result)
                {
                    // Update RecentFile list for successfully loaded files only. 
                    UpdateRecentFiles(this.recentFileManager.AddFile(ViewModel.Statement.StorageKey));
                }
            }
            catch (FileNotFoundException ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.FileName))
                {
                    // Remove the bad file that caused the exception from the RecentFiles list.
                    UpdateRecentFiles(this.recentFileManager.Remove(ex.FileName));
                }
            }
        }

        private void OnRecentFileManagerStateRestored(object sender, EventArgs e)
        {
            UpdateRecentFiles();
        }

        private async void OnSaveStatementExecute()
        {
            // TODO reassess this - because saving of data async while user edits are taking place will result in inconsistent results.
            await SaveAsync(false);
            UpdateRecentFiles(this.recentFileManager.UpdateFile(ViewModel.Statement.StorageKey));
        }

        private bool PromptToSaveIfDirty()
        {
            if (ViewModel.Statement != null && ViewModel.Dirty)
            {
                bool? result = this.yesNoBox.Show(
                    "Statement has been modified, save changes?",
                    "Budget Analyser");
                if (result != null && result.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task SaveAsync(bool close)
        {
            await this.transactionService.SaveAsync(close);
            ViewModel.TriggerRefreshTotalsRow();
            NotifyOfReset();
        }

        private void UpdateRecentFiles(IEnumerable<KeyValuePair<string, string>> files)
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            //this.recentFileCommands =
            //    files.Select(
            //        f => (ICommand)new RecentFileRelayCommand(
            //            f.Value,
            //            f.Key,
            //            OnOpenStatementExecuteAsync,
            //            CanExecuteRecentFileOpenCommand))
            //        .ToList();
            //RaisePropertyChanged(() => RecentFile1Command);
            //RaisePropertyChanged(() => RecentFile2Command);
            //RaisePropertyChanged(() => RecentFile3Command);
            //RaisePropertyChanged(() => RecentFile4Command);
            //RaisePropertyChanged(() => RecentFile5Command);
        }
    }
}