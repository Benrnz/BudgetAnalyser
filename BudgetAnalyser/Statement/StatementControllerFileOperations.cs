using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser.Statement
{
    public class StatementControllerFileOperations : ViewModelBase
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly LoadFileController loadFileController;
        private readonly IRecentFileManager recentFileManager;
        private readonly IStatementRepository statementRepository;
        private bool doNotUseLoadingData;
        private List<ICommand> recentFileCommands;
        private readonly IUserMessageBox messageBox;
        private readonly IUserQuestionBoxYesNo yesNoBox;

        public StatementControllerFileOperations(
            [NotNull] IUiContext uiContext,
            [NotNull] IStatementRepository statementRepository,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] DemoFileHelper demoFileHelper,
            [NotNull] LoadFileController loadFileController,
            [NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statementRepository == null)
            {
                throw new ArgumentNullException("statementRepository");
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

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.yesNoBox = uiContext.UserPrompts.YesNoBox;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.statementRepository = statementRepository;
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;
            this.loadFileController = loadFileController;
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

        public bool LoadingData
        {
            get { return this.doNotUseLoadingData; }
            private set
            {
                this.doNotUseLoadingData = value;
                RaisePropertyChanged(() => LoadingData);
            }
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

        private void FileCannotBeLoaded(Exception ex)
        {
            this.messageBox.Show("The file cannot be loaded.\n" + ex.Message);
        }

        public ICommand MergeStatementCommand
        {
            get { return new RelayCommand(OnMergeStatementCommandExecute, CanExecuteCloseStatementCommand); }
        }

        public ICommand OpenStatementCommand
        {
            get { return new RelayCommand(() => OnOpenStatementExecuteAsync(null), CanExecuteOpenStatementCommand); }
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

        public async void NotifyOfClosingAsync()
        {
            if (PromptToSaveIfDirty())
            {
                await SaveAsync();
            }
        }

        internal void Initialise(StatementController controller)
        {
            ViewModel.Initialise(controller);
        }

        internal async Task LoadStatementFromApplicationStateAsync(string statementFileName)
        {
            try
            {
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

                await LoadInternalAsync(statementFileName);
                WaitingForBudgetToLoad = null;
            }
            catch (KeyNotFoundException)
            {
                // Ignore it.
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
            return ViewModel.Statement != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return !LoadingData;
        }

        private async Task<bool> LoadInternalAsync(string fullFileName)
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
                statementModel = await this.statementRepository.LoadStatementModelAsync(fullFileName);
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

            LoadingData = false;
            return true;
        }

        private void NotifyOfReset()
        {
            ViewModel.Dirty = false;
            MessengerInstance.Send(new StatementHasBeenModifiedMessage(false, ViewModel.Statement));
        }

        private async void OnCloseStatementExecute()
        {
            if (PromptToSaveIfDirty())
            {
                await SaveAsync();
            }

            ViewModel.Statement = null;
            NotifyOfReset();
            ViewModel.TriggerRefreshTotalsRow();
            MessengerInstance.Send(new StatementReadyMessage(null));
        }

        private void OnDemoStatementCommandExecuted()
        {
            OnOpenStatementExecuteAsync(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
        }

        private async void OnMergeStatementCommandExecute()
        {
            await SaveAsync();
            ViewModel.BucketFilter = null;

            var fileName = await GetFileNameFromUser(StatementOpenMode.Merge);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled
                return;
            }

            StatementModel additionalModel = null;
            try
            {
                var account = this.loadFileController.SelectedExistingAccountName;
                additionalModel = this.statementRepository.ImportAndMergeBankStatementAsync(fileName, account);

                if (additionalModel == null)
                {
                    // User cancelled.
                    return;
                }

                ViewModel.Statement.Merge(additionalModel);

                RaisePropertyChanged(() => ViewModel);
                MessengerInstance.Send(new TransactionsChangedMessage());
                NotifyOfEdit();
                ViewModel.TriggerRefreshTotalsRow();
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
                if (additionalModel != null)
                {
                    MessengerInstance.Send(new StatementReadyMessage(ViewModel.Statement));
                }

                this.loadFileController.Reset();
            }
        }

        private async void OnOpenStatementExecuteAsync(string fullFileName)
        {
            if (PromptToSaveIfDirty())
            {
                await SaveAsync();
            }

            // Will prompt for file name if its null, which it will be for clicking the Load button, but RecentFilesButtons also use this method which will have a filename.
            try
            {
                bool result = await LoadInternalAsync(fullFileName);

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

        private async void OnSaveStatementExecute()
        {
            // TODO reassess this - because saving of data async while user edits are taking place will result in inconsistent results.
            await SaveAsync();
            UpdateRecentFiles(this.recentFileManager.UpdateFile(ViewModel.Statement.StorageKey));
        }

        private bool PromptToSaveIfDirty()
        {
            if (ViewModel.Statement != null && ViewModel.Dirty)
            {
                bool? result = this.yesNoBox.Show("Statement has been modified, save changes?",
                    "Budget Analyser");
                if (result != null && result.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task SaveAsync()
        {
            await this.statementRepository.SaveAsync(ViewModel.Statement);
            ViewModel.TriggerRefreshTotalsRow();
            NotifyOfReset();
        }

        private void UpdateRecentFiles(IEnumerable<KeyValuePair<string, string>> files)
        {
            this.recentFileCommands =
                files.Select(f => (ICommand)new RecentFileRelayCommand(f.Value, f.Key, OnOpenStatementExecuteAsync, x => CanExecuteOpenStatementCommand()))
                    .ToList();
            RaisePropertyChanged(() => RecentFile1Command);
            RaisePropertyChanged(() => RecentFile2Command);
            RaisePropertyChanged(() => RecentFile3Command);
            RaisePropertyChanged(() => RecentFile4Command);
            RaisePropertyChanged(() => RecentFile5Command);
        }
    }
}