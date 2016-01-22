using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerBookController : ControllerBase, IShowableController
    {
        private readonly IUserInputBox inputBox;
        private readonly ILedgerService ledgerService;
        private readonly IReconciliationService reconService;
        private readonly IUserMessageBox messageBox;
        private readonly NewWindowViewLoader newWindowViewLoader;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly LedgerBookGridBuilderFactory uiBuilder;
        private readonly UiContext uiContext;
        private int doNotUseNumberOfMonthsToShow;
        private bool doNotUseShown;

        public LedgerBookController(
            [NotNull] UiContext uiContext,
            [NotNull] LedgerBookControllerFileOperations fileOperations,
            [NotNull] LedgerBookGridBuilderFactory uiBuilder,
            [NotNull] ILedgerService ledgerService,
            [NotNull] IReconciliationService reconService,
            [NotNull] NewWindowViewLoader newWindowViewLoader)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (fileOperations == null)
            {
                throw new ArgumentNullException(nameof(fileOperations));
            }

            if (uiBuilder == null)
            {
                throw new ArgumentNullException(nameof(uiBuilder));
            }

            if (ledgerService == null)
            {
                throw new ArgumentNullException(nameof(ledgerService));
            }

            if (reconService == null)
            {
                throw new ArgumentNullException(nameof(reconService));
            }

            if (newWindowViewLoader == null)
            {
                throw new ArgumentNullException(nameof(newWindowViewLoader));
            }

            this.uiBuilder = uiBuilder;
            this.ledgerService = ledgerService;
            this.reconService = reconService;
            this.newWindowViewLoader = newWindowViewLoader;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.inputBox = uiContext.UserPrompts.InputBox;
            FileOperations = fileOperations;
            FileOperations.LedgerService = this.ledgerService;
            this.uiContext = uiContext;
            this.doNotUseNumberOfMonthsToShow = 2;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);

            this.ledgerService.Saved += OnSaveNotificationReceieved;
            this.ledgerService.Closed += OnClosedNotificationReceived;
            this.ledgerService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        }

        public event EventHandler LedgerBookUpdated;

        [UsedImplicitly]
        public ICommand AddNewLedgerCommand => new RelayCommand(OnAddNewLedgerCommandExecuted, CanExecuteAddNewLedgerCommand);

        [UsedImplicitly]
        public ICommand AddNewReconciliationCommand => new RelayCommand(OnAddNewReconciliationCommandExecuted, CanExecuteNewReconciliationCommand);

        public LedgerBookControllerFileOperations FileOperations { get; }

        public int NumberOfMonthsToShow
        {
            get { return this.doNotUseNumberOfMonthsToShow; }
            set
            {
                this.doNotUseNumberOfMonthsToShow = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RemoveLedgerEntryLineCommand => new RelayCommand<LedgerEntryLine>(OnRemoveReconciliationCommandExecuted, CanExecuteRemoveLedgerEntryLineCommand);
        public ICommand ShowBankBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowBankBalancesCommandExecuted, param => param != null);
        public ICommand ShowHideMonthsCommand => new RelayCommand<int>(OnShowHideMonthsCommandExecuted);
        public ICommand ShowLedgerBucketDetailsCommand => new RelayCommand<LedgerBucket>(OnShowLedgerBucketDetailsCommand, param => param != null);

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
                RaisePropertyChanged();
            }
        }

        public ICommand ShowRemarksCommand => new RelayCommand<LedgerEntryLine>(OnShowRemarksCommandExecuted, CanExecuteShowRemarksCommand);
        public ICommand ShowSurplusBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowSurplusBalancesCommandExecuted, param => param != null);
        public ICommand ShowTransactionsCommand => new RelayCommand<object>(OnShowTransactionsCommandExecuted);
        public ReconciliationToDoListController ToDoListController => this.uiContext.ReconciliationToDoListController;

        [UsedImplicitly]
        public ICommand TransferFundsCommand => new RelayCommand(OnTransferFundsCommandExecuted, CanExecuteTransferFundsCommand);

        [UsedImplicitly]
        public ICommand UnlockLedgerLineCommand => new RelayCommand(OnUnlockLedgerLineCommandExecuted, CanExecuteUnlockLedgerLineCommand);

        public LedgerBookViewModel ViewModel => FileOperations.ViewModel;

        public void DeregisterListener<T>(object listener, Action<T> handler)
        {
            MessengerInstance.Unregister(listener, handler);
        }

        public void EditLedgerBookName()
        {
            if (ViewModel.LedgerBook == null)
            {
                return;
            }

            string result = this.inputBox.Show("Edit Ledger Book Name", "Enter a new name", ViewModel.LedgerBook.Name);
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            this.ledgerService.RenameLedgerBook(result);
            FileOperations.Dirty = true;
        }

        public void RegisterListener<T>(object listener, Action<T> handler)
        {
            MessengerInstance.Register(listener, handler);
        }

        internal ILedgerBookGridBuilder GridBuilder()
        {
            return this.uiBuilder.GridBuilderV2(this);
        }

        private bool CanExecuteAddNewLedgerCommand()
        {
            return ViewModel.LedgerBook != null;
        }

        private bool CanExecuteNewReconciliationCommand()
        {
            return ViewModel.CurrentBudget != null && ViewModel.CurrentStatement != null && ViewModel.LedgerBook != null;
        }

        private bool CanExecuteRemoveLedgerEntryLineCommand(LedgerEntryLine line)
        {
            return ViewModel.LedgerBook != null && ViewModel.LedgerBook.Reconciliations.FirstOrDefault() == line && line == ViewModel.NewLedgerLine;
        }

        private bool CanExecuteShowRemarksCommand(LedgerEntryLine parameter)
        {
            return parameter != null
                   && (!string.IsNullOrWhiteSpace(parameter.Remarks) || parameter == ViewModel.NewLedgerLine);
        }

        private bool CanExecuteTransferFundsCommand()
        {
            return CanExecuteNewReconciliationCommand() && ViewModel.NewLedgerLine != null;
        }

        private bool CanExecuteUnlockLedgerLineCommand()
        {
            return ViewModel.NewLedgerLine == null && ViewModel.LedgerBook != null;
        }

        private void FinaliseAddingReconciliation(bool ignoreWarnings = false)
        {
            try
            {
                ViewModel.NewLedgerLine = this.reconService.MonthEndReconciliation(
                    ViewModel.LedgerBook,
                    this.uiContext.AddLedgerReconciliationController.Date,
                    ViewModel.CurrentBudget,
                    ViewModel.CurrentStatement,
                    ignoreWarnings, 
                    this.uiContext.AddLedgerReconciliationController.BankBalances.Cast<BankBalance>().ToArray());

                FileOperations.ReconciliationChangesWillNeedToBeSaved();
                NumberOfMonthsToShow++;
                RaiseLedgerBookUpdated();
                if (this.reconService.ReconciliationToDoList.Any())
                {
                    ToDoListController.Load(this.reconService.ReconciliationToDoList);
                    this.newWindowViewLoader.Show(ToDoListController);
                }
            }
            catch (ValidationWarningException ex)
            {
                if (!ProceedAfterReconciliationValidationWarning(ex))
                {
                    return;
                }

                FinaliseAddingReconciliation(true);
            }
            catch (InvalidOperationException ex)
            {
                this.messageBox.Show(ex, "Unable to add new reconciliation.");
            }
        }

        private void OnAddNewLedgerCommandExecuted()
        {
            this.uiContext.ChooseBudgetBucketController.Chosen += OnAddNewLedgerComplete;
            this.uiContext.ChooseBudgetBucketController.Filter(bucket => bucket is ExpenseBucket, "Choose an Expense Budget Bucket");
            this.uiContext.ChooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.LedgerBook, "Add New Ledger to Ledger Book", Guid.NewGuid(), true);
        }

        private void OnAddNewLedgerComplete(object sender, BudgetBucketChosenEventArgs e)
        {
            this.uiContext.ChooseBudgetBucketController.Chosen -= OnAddNewLedgerComplete;
            if (e.Canceled)
            {
                return;
            }

            var expenseBucket = e.SelectedBucket as ExpenseBucket;
            if (expenseBucket == null)
            {
                this.messageBox.Show("You must select an expense budget bucket to track when adding a new Ledger Column.");
                return;
            }

            this.ledgerService.TrackNewBudgetBucket(expenseBucket, e.StoreInThisAccount);
        }

        private void OnAddNewReconciliationCommandExecuted()
        {
            try
            {
                this.reconService.BeforeReconciliationValidation(ViewModel.LedgerBook, ViewModel.CurrentStatement);
            }
            catch (ValidationWarningException ex)
            {
                if (!ProceedAfterReconciliationValidationWarning(ex))
                {
                    return;
                }
            }

            this.uiContext.AddLedgerReconciliationController.Complete += OnAddReconciliationComplete;
            this.uiContext.AddLedgerReconciliationController.ShowCreateDialog(ViewModel.LedgerBook);
        }

        private void OnAddReconciliationComplete(object sender, EditBankBalancesEventArgs e)
        {
            this.uiContext.AddLedgerReconciliationController.Complete -= OnAddReconciliationComplete;

            if (e.Canceled)
            {
                return;
            }

            FinaliseAddingReconciliation();
        }

        private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
        {
            if (message.ActiveBudget == null)
            {
                ViewModel.CurrentBudget = null;
                return;
            }

            if (message.ActiveBudget.BudgetActive)
            {
                ViewModel.CurrentBudget = message.ActiveBudget;
            }
        }

        private void OnClosedNotificationReceived(object sender, EventArgs eventArgs)
        {
            FileOperations.Close();
        }

        private void OnLedgerBucketUpdated(object sender, EventArgs e)
        {
            this.uiContext.LedgerBucketViewController.Updated -= OnLedgerBucketUpdated;
            RaiseLedgerBookUpdated();
            FileOperations.Dirty = true;
        }

        private void OnNewDataSourceAvailableNotificationReceived(object sender, EventArgs eventArgs)
        {
            FileOperations.SyncDataFromLedgerService();
        }

        private void OnRemoveReconciliationCommandExecuted(LedgerEntryLine line)
        {
            bool? result = this.questionBox.Show(
                string.Format(CultureInfo.CurrentCulture, "Are you sure you want to delete this Reconciliation for {0:d}?\nThis will also save the Ledger Book.", line.Date),
                "Remove Ledger Book Reconciliation");
            if (result == null || !result.Value)
            {
                return;
            }

            NumberOfMonthsToShow--;
            this.ledgerService.RemoveReconciliation(line);
            FileOperations.SyncDataFromLedgerService();
            EventHandler handler = LedgerBookUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveNotificationReceieved(object sender, EventArgs eventArgs)
        {
            ViewModel.NewLedgerLine = null;
        }

        private void OnShowBankBalancesCommandExecuted(LedgerEntryLine line)
        {
            this.uiContext.AddLedgerReconciliationController.ShowEditDialog(ViewModel.LedgerBook, line);
        }

        private void OnShowHideMonthsCommandExecuted(int increment)
        {
            NumberOfMonthsToShow += increment;
            MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
        }

        private void OnShowLedgerBucketDetailsCommand(LedgerBucket ledgerBucket)
        {
            this.uiContext.LedgerBucketViewController.Updated += OnLedgerBucketUpdated;
            this.uiContext.LedgerBucketViewController.ShowDialog(ViewModel.LedgerBook, ledgerBucket, ViewModel.CurrentBudget.Model);
        }

        private void OnShowRemarksCommandExecuted(LedgerEntryLine parameter)
        {
            this.uiContext.LedgerRemarksController.Completed += OnShowRemarksCompleted;
            this.uiContext.LedgerRemarksController.Show(parameter, parameter == ViewModel.NewLedgerLine);
        }

        private void OnShowRemarksCompleted(object sender, EventArgs e)
        {
            this.uiContext.LedgerRemarksController.Completed -= OnShowRemarksCompleted;
        }

        private void OnShowSurplusBalancesCommandExecuted(LedgerEntryLine line)
        {
            this.uiContext.ShowSurplusBalancesController.ShowDialog(line);
        }

        private void OnShowTransactionsCommandExecuted(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            this.uiContext.LedgerTransactionsController.Complete += OnShowTransactionsCompleted;

            var ledgerEntry = parameter as LedgerEntry; // used when legder entry transactions are being displayed
            var bankBalanceAdjustments = parameter as LedgerEntryLine; // used when balance adjustments are being displayed

            if (ledgerEntry != null)
            {
                bool isNew = ViewModel.NewLedgerLine != null && ViewModel.NewLedgerLine.Entries.Any(e => e == ledgerEntry);
                this.uiContext.LedgerTransactionsController.ShowDialog(ViewModel.NewLedgerLine, ledgerEntry, isNew);
            }
            else if (bankBalanceAdjustments != null)
            {
                this.uiContext.LedgerTransactionsController.ShowDialog(bankBalanceAdjustments, bankBalanceAdjustments == ViewModel.NewLedgerLine);
            }
            else
            {
                throw new ArgumentException("Invalid paramter passed to ShowTransactionsCommand: " + parameter);
            }
        }

        private void OnShowTransactionsCompleted(object sender, LedgerTransactionEventArgs args)
        {
            this.uiContext.LedgerTransactionsController.Complete -= OnShowTransactionsCompleted;

            if (args.WasModified)
            {
                RaiseLedgerBookUpdated();
            }
        }

        private void OnStatementReadyMessageReceived(StatementReadyMessage message)
        {
            ViewModel.CurrentStatement = message.StatementModel;
        }

        private void OnTransferFundsCommandExecuted()
        {
            this.uiContext.TransferFundsController.TransferFundsRequested += OnTransferFundsRequested;
            this.uiContext.TransferFundsController.ShowDialog(ViewModel.LedgerBook.LedgersAvailableForTransfer());
        }

        private void OnTransferFundsRequested(object sender, EventArgs eventArgs)
        {
            this.uiContext.TransferFundsController.TransferFundsRequested -= OnTransferFundsRequested;
            this.reconService.TransferFunds(ViewModel.NewLedgerLine, this.uiContext.TransferFundsController.TransferFundsDto);
            RaiseLedgerBookUpdated();
            FileOperations.ReconciliationChangesWillNeedToBeSaved();
        }

        private void OnUnlockLedgerLineCommandExecuted()
        {
            bool? response = this.questionBox.Show(
                "Unlock Ledger Entry Line",
                "Are you sure you want to unlock the Ledger Entry Line dated {0:d} for editing?",
                ViewModel.LedgerBook.Reconciliations.First().Date);

            if (response == null || response.Value == false)
            {
                return;
            }

            ViewModel.NewLedgerLine = this.reconService.UnlockCurrentMonth(ViewModel.LedgerBook);
            FileOperations.Dirty = true;
        }

        private bool ProceedAfterReconciliationValidationWarning(ValidationWarningException ex)
        {
            bool? result = this.questionBox.Show("Warning", "Warning: {0}\nDo you wish to proceed?", ex.Message);
            return result != null && result.Value;
        }

        private void RaiseLedgerBookUpdated()
        {
            EventHandler handler = LedgerBookUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}