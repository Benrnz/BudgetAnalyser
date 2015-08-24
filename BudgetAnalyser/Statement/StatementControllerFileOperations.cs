using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class StatementControllerFileOperations : ViewModelBase
    {
        private readonly LoadFileController loadFileController;
        private readonly IUserMessageBox messageBox;
        private bool doNotUseLoadingData;
        private ITransactionManagerService transactionService;

        public StatementControllerFileOperations(
            [NotNull] IUiContext uiContext,
            [NotNull] LoadFileController loadFileController,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (loadFileController == null)
            {
                throw new ArgumentNullException(nameof(loadFileController));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.loadFileController = loadFileController;
            ViewModel = new StatementViewModel(uiContext, applicationDatabaseService);
        }

        public bool LoadingData
        {
            [UsedImplicitly] get { return this.doNotUseLoadingData; }
            private set
            {
                this.doNotUseLoadingData = value;
                RaisePropertyChanged();
            }
        }

        internal StatementViewModel ViewModel { get; }

        internal bool CanExecuteCloseStatementCommand()
        {
            return ViewModel.Statement != null;
        }

        internal void Close()
        {
            ViewModel.Statement = null;
            NotifyOfReset();
            ViewModel.TriggerRefreshTotalsRow();
            MessengerInstance.Send(new StatementReadyMessage(null));
        }

        internal void Initialise(ITransactionManagerService transactionManagerService)
        {
            this.transactionService = transactionManagerService;
            ViewModel.Initialise(this.transactionService);
        }

        internal async Task MergeInNewTransactions()
        {
            PersistenceOperationCommands.SaveDatabaseCommand.Execute(this);

            string fileName = await GetFileNameFromUser(StatementOpenMode.Merge);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled
                return;
            }

            try
            {
                Account account = this.loadFileController.SelectedExistingAccountName;
                await this.transactionService.ImportAndMergeBankStatementAsync(fileName, account);

                await SyncWithServiceAsync();
                MessengerInstance.Send(new TransactionsChangedMessage());
                RaisePropertyChanged(() => ViewModel);
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

        internal async Task<bool> SyncWithServiceAsync()
        {
            StatementModel statementModel = this.transactionService.StatementModel;
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
    }
}