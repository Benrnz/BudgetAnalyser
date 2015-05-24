using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    /// A controller for editing transactions and balance adjustments.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerTransactionsController : ControllerBase
    {
        private readonly ILedgerService ledgerService;
        private Guid dialogCorrelationId;
        private bool doNotUseIsReadOnly;
        private LedgerEntry doNotUseLedgerEntry;
        private decimal doNotUseNewTransactionAmount;
        private string doNotUseNewTransactionNarrative;
        private bool doNotUseShowAddingNewTransactionPanel;
        private string doNotUseTitle;
        private LedgerEntryLine entryLine;
        private bool isAddDirty;
        private bool wasChanged;
        private AccountType doNotUseNewTransactionAccountType;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LedgerTransactionsController([NotNull] UiContext uiContext, [NotNull] ILedgerService ledgerService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (ledgerService == null)
            {
                throw new ArgumentNullException("ledgerService");
            }

            this.ledgerService = ledgerService;
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            Reset();
        }

        public event EventHandler<LedgerTransactionEventArgs> Complete;

        public IEnumerable<AccountType> AccountTypes
        {
            get { return this.ledgerService.ValidLedgerAccounts(); } 
        }

        public ICommand AddTransactionCommand
        {
            get { return new RelayCommand(OnAddNewTransactionCommandExecuted, CanExecuteAddTransactionCommand); }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return new RelayCommand<LedgerTransaction>(OnDeleteTransactionCommandExecuted, CanExecuteDeleteTransactionCommand); }
        }

        public bool InBalanceAdjustmentMode
        {
            get { return LedgerEntry == null; }
        }

        public bool InLedgerEntryMode
        {
            get { return LedgerEntry != null; }
        }

        public bool IsReadOnly
        {
            get { return this.doNotUseIsReadOnly; }
            set
            {
                this.doNotUseIsReadOnly = value;
                RaisePropertyChanged();
            }
        }

        public LedgerEntry LedgerEntry
        {
            get { return this.doNotUseLedgerEntry; }
            private set
            {
                this.doNotUseLedgerEntry = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => InBalanceAdjustmentMode);
                RaisePropertyChanged(() => InLedgerEntryMode);
            }
        }

        public AccountType NewTransactionAccountType
        {
            get { return this.doNotUseNewTransactionAccountType; }
            set
            {
                this.doNotUseNewTransactionAccountType = value;
                RaisePropertyChanged();
            }
        }

        public decimal NewTransactionAmount
        {
            get { return this.doNotUseNewTransactionAmount; }
            set
            {
                this.doNotUseNewTransactionAmount = value;
                RaisePropertyChanged();
            }
        }

        public string NewTransactionNarrative
        {
            get { return this.doNotUseNewTransactionNarrative; }
            set
            {
                this.doNotUseNewTransactionNarrative = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowAddingNewTransactionPanel
        {
            get { return this.doNotUseShowAddingNewTransactionPanel; }
            private set
            {
                this.doNotUseShowAddingNewTransactionPanel = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<LedgerTransaction> ShownTransactions { get; private set; }

        public string Title
        {
            get { return this.doNotUseTitle; }
            private set
            {
                this.doNotUseTitle = value;
                RaisePropertyChanged();
            }
        }

        public decimal TransactionsTotal
        {
            get { return ShownTransactions == null ? 0 : ShownTransactions.Sum(t => t.Amount); }
        }

        public ICommand ZeroNetAmountCommand
        {
            get { return new RelayCommand(OnZeroNetAmountCommandExecuted, CanExecuteZeroNetAmountCommand); }
        }

        /// <summary>
        ///     Show the Ledger Transactions view for viewing and editing Ledger Transactions.
        /// </summary>
        public void ShowDialog(LedgerEntry ledgerEntry, bool isNew)
        {
            if (ledgerEntry == null)
            {
                return;
            }

            LedgerEntry = ledgerEntry;
            this.entryLine = null;
            ShownTransactions = new ObservableCollection<LedgerTransaction>(LedgerEntry.Transactions);
            Title = string.Format(CultureInfo.CurrentCulture, "{0} Transactions", ledgerEntry.LedgerBucket.BudgetBucket.Code);
            ShowDialogCommon(isNew);
        }

        /// <summary>
        ///     Show the Ledger Transactions view, for viewing and editing Balance Adjustments
        /// </summary>
        public void ShowDialog(LedgerEntryLine ledgerEntryLine, bool isNew)
        {
            if (ledgerEntryLine == null)
            {
                return;
            }

            LedgerEntry = null;
            this.entryLine = ledgerEntryLine;
            ShownTransactions = new ObservableCollection<LedgerTransaction>(ledgerEntryLine.BankBalanceAdjustments);
            Title = "Balance Adjustment Transactions";
            ShowDialogCommon(isNew);
        }

        private bool CanExecuteAddTransactionCommand()
        {
            return !IsReadOnly;
        }

        private bool CanExecuteDeleteTransactionCommand(LedgerTransaction arg)
        {
            return !IsReadOnly && arg != null;
        }

        private bool CanExecuteZeroNetAmountCommand()
        {
            return LedgerEntry != null && LedgerEntry.NetAmount != 0;
        }

        private void OnAddNewTransactionCommandExecuted()
        {
            // This command is executed to show the add transaction panel, then again to add the transaction once the edit fields are completed.
            if (ShowAddingNewTransactionPanel)
            {
                ShowAddingNewTransactionPanel = false;
                this.isAddDirty = true;
                Save();
            }
            else
            {
                ShowAddingNewTransactionPanel = true;
            }
        }

        private void OnDeleteTransactionCommandExecuted(LedgerTransaction transaction)
        {
            if (IsReadOnly)
            {
                return;
            }

            if (InLedgerEntryMode)
            {
                this.wasChanged = true;
                this.ledgerService.RemoveTransaction(LedgerEntry, transaction.Id);
                ShownTransactions.Remove(transaction);
            }
            else if (InBalanceAdjustmentMode)
            {
                this.wasChanged = true;
                this.ledgerService.CancelBalanceAdjustment(this.entryLine, transaction.Id);
                ShownTransactions.Remove(transaction);
            }

            RaisePropertyChanged(() => TransactionsTotal);
            RaisePropertyChanged(() => LedgerEntry);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Ok)
            {
                if (ShowAddingNewTransactionPanel)
                {
                    OnAddNewTransactionCommandExecuted();
                }

                Save();
                this.entryLine = null;
                LedgerEntry = null;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                this.isAddDirty = false;
                this.entryLine = null;
                LedgerEntry = null;
            }

            EventHandler<LedgerTransactionEventArgs> handler = Complete;
            if (handler != null)
            {
                handler(this, new LedgerTransactionEventArgs(this.wasChanged));
            }

            Reset();
            this.wasChanged = false;
        }

        private void OnZeroNetAmountCommandExecuted()
        {
            if (LedgerEntry == null)
            {
                return;
            }

            if (LedgerEntry.NetAmount == 0)
            {
                return;
            }

            if (LedgerEntry.NetAmount > 0)
            {
                NewTransactionNarrative = "Zero the remainder - don't accumulate credits";
                NewTransactionAmount = LedgerEntry.NetAmount;
            }
            else
            {
                NewTransactionNarrative = "Zero the remainder - supplement shortfall from surplus";
                NewTransactionAmount = -LedgerEntry.NetAmount;
            }
        }

        private void Reset()
        {
            ShowAddingNewTransactionPanel = false;
            this.isAddDirty = false;
            // LedgerEntry = null;    // Dont reset this here.  If the user is adding multiple transactions this will prevent adding any more transactions.
            // this.entryLine = null; // Dont reset this here.  If the user is adding multiple transactions this will prevent adding any more transactions.
            NewTransactionAmount = 0;
            NewTransactionNarrative = null;
            NewTransactionAccountType = null;
        }

        private void Save()
        {
            if (IsReadOnly)
            {
                return;
            }

            if (InBalanceAdjustmentMode && this.isAddDirty)
            {
                SaveBalanceAdjustment();
            }
            else if (InLedgerEntryMode && this.isAddDirty)
            {
                SaveNewEntryTransaction();
            }

            Reset();
            RaisePropertyChanged(() => LedgerEntry);
        }

        private void SaveBalanceAdjustment()
        {
            var newTransaction = this.ledgerService.CreateBalanceAdjustment(this.entryLine, NewTransactionAmount, NewTransactionNarrative, NewTransactionAccountType);
            ShownTransactions.Add(newTransaction);
            this.wasChanged = true;
            RaisePropertyChanged(() => TransactionsTotal);
        }

        private void SaveNewEntryTransaction()
        {
            try
            {
                var newTransaction = this.ledgerService.CreateLedgerTransaction(LedgerEntry, NewTransactionAmount, NewTransactionNarrative);
                ShownTransactions.Add(newTransaction);
            }
            catch (ArgumentException)
            {
                // Invalid transaction data
                return;
            }

            RaisePropertyChanged(() => TransactionsTotal);
            this.wasChanged = true;
        }

        private void ShowDialogCommon(bool isNew)
        {
            IsReadOnly = !isNew;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, IsReadOnly ? ShellDialogType.Ok : ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = Title,
            };
            MessengerInstance.Send(dialogRequest);
        }
    }
}