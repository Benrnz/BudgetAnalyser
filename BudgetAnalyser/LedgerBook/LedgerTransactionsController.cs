using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerTransactionsController : ControllerBase
    {
        private Guid dialogCorrelationId;
        private bool doNotUseAddingNewTransaction;
        private bool doNotUseIsReadOnly;
        private LedgerEntry doNotUseLedgerEntry;
        private decimal doNotUseNewTransactionAmount;
        private bool doNotUseNewTransactionIsCredit;
        private bool doNotUseNewTransactionIsDebit;
        private bool doNotUseNewTransactionIsReversal;
        private string doNotUseNewTransactionNarrative;
        private IEnumerable<LedgerTransaction> doNotUseShownTransactions;
        private string doNotUseTitle;
        private LedgerEntryLine entryLine;
        private bool isAddDirty;
        private bool isDeleteDirty;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LedgerTransactionsController([NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            Reset();
        }

        public event EventHandler<LedgerTransactionEventArgs> Complete;

        public ICommand AddTransactionCommand
        {
            get { return new RelayCommand(OnAddNewTransactionCommandExecuted, CanExecuteAddTransactionCommand); }
        }

        public bool AddingNewTransaction
        {
            get { return this.doNotUseAddingNewTransaction; }
            private set
            {
                this.doNotUseAddingNewTransaction = value;
                RaisePropertyChanged(() => AddingNewTransaction);
            }
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
                RaisePropertyChanged(() => IsReadOnly);
            }
        }

        public LedgerEntry LedgerEntry
        {
            get { return this.doNotUseLedgerEntry; }
            private set
            {
                this.doNotUseLedgerEntry = value;
                RaisePropertyChanged(() => LedgerEntry);
                RaisePropertyChanged(() => InBalanceAdjustmentMode);
                RaisePropertyChanged(() => InLedgerEntryMode);
            }
        }

        public decimal NewTransactionAmount
        {
            get { return this.doNotUseNewTransactionAmount; }
            set
            {
                this.doNotUseNewTransactionAmount = value;
                RaisePropertyChanged(() => NewTransactionAmount);
            }
        }

        public bool NewTransactionIsCredit
        {
            get { return this.doNotUseNewTransactionIsCredit; }
            set
            {
                this.doNotUseNewTransactionIsCredit = value;
                RaisePropertyChanged(() => NewTransactionIsCredit);
            }
        }

        public bool NewTransactionIsDebit
        {
            get { return this.doNotUseNewTransactionIsDebit; }
            set
            {
                this.doNotUseNewTransactionIsDebit = value;
                RaisePropertyChanged(() => NewTransactionIsDebit);
            }
        }

        public bool NewTransactionIsReversal
        {
            get { return this.doNotUseNewTransactionIsReversal; }
            set
            {
                this.doNotUseNewTransactionIsReversal = value;
                RaisePropertyChanged(() => NewTransactionIsReversal);
            }
        }

        public string NewTransactionNarrative
        {
            get { return this.doNotUseNewTransactionNarrative; }
            set
            {
                this.doNotUseNewTransactionNarrative = value;
                RaisePropertyChanged(() => NewTransactionNarrative);
            }
        }

        public IEnumerable<LedgerTransaction> ShownTransactions
        {
            get { return this.doNotUseShownTransactions; }
            private set
            {
                this.doNotUseShownTransactions = value;
                RaisePropertyChanged(() => ShownTransactions);
                RaisePropertyChanged(() => TransactionsTotal);
            }
        }

        public string Title
        {
            get { return this.doNotUseTitle; }
            private set
            {
                this.doNotUseTitle = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public decimal TransactionsTotal
        {
            get { return ShownTransactions == null ? 0 : ShownTransactions.Sum(t => t.Credit - t.Debit); }
        }

        public ICommand ZeroNetAmountCommand
        {
            get { return new RelayCommand(OnZeroNetAmountCommandExecuted, CanExecuteZeroNetAmountCommand); }
        }

        /// <summary>
        ///     Show the Ledger Transactions view for viewing and editing Ledger Transactions.
        /// </summary>
        /// <param name="ledgerEntry"></param>
        /// <param name="isNew"></param>
        public void ShowDialog(LedgerEntry ledgerEntry, bool isNew)
        {
            if (ledgerEntry == null)
            {
                return;
            }

            LedgerEntry = ledgerEntry;
            ShownTransactions = LedgerEntry.Transactions;
            Title = string.Format(CultureInfo.CurrentCulture, "{0} Transactions", ledgerEntry.LedgerColumn.BudgetBucket.Code);
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
            ShownTransactions = ledgerEntryLine.BankBalanceAdjustments;
            Title = "Balance Adjustment Transactions";
            this.entryLine = ledgerEntryLine;
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
            if (AddingNewTransaction)
            {
                AddingNewTransaction = false;
                this.isAddDirty = true;
                Save();
            }
            else
            {
                AddingNewTransaction = true;
            }
        }

        private void OnDeleteTransactionCommandExecuted(LedgerTransaction obj)
        {
            if (IsReadOnly)
            {
                return;
            }

            this.isDeleteDirty = true;
            if (InLedgerEntryMode)
            {
                LedgerEntry.RemoveTransaction(obj.Id);
                ShownTransactions = LedgerEntry.Transactions.ToList();
            }
            else if (InBalanceAdjustmentMode)
            {
                this.entryLine.CancelBalanceAdjustment(obj.Id);
                ShownTransactions = this.entryLine.BankBalanceAdjustments.ToList();
            }

            RaisePropertyChanged(() => LedgerEntry);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (message.CorrelationId != this.dialogCorrelationId)
            {
                return;
            }

            if (AddingNewTransaction)
            {
                OnAddNewTransactionCommandExecuted();
            }

            Save();

            EventHandler<LedgerTransactionEventArgs> handler = Complete;
            if (handler != null)
            {
                handler(this, new LedgerTransactionEventArgs(this.isAddDirty || this.isDeleteDirty));
            }

            Reset();
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
                NewTransactionIsDebit = true;
                NewTransactionIsCredit = false;
                NewTransactionNarrative = "Zero the remainder - don't accumulate credits";
                NewTransactionAmount = LedgerEntry.NetAmount;
            }
            else
            {
                NewTransactionIsDebit = false;
                NewTransactionIsCredit = true;
                NewTransactionNarrative = "Zero the remainder - supplement shortfall from surplus";
                NewTransactionAmount = -LedgerEntry.NetAmount;
            }
        }

        private void Reset()
        {
            AddingNewTransaction = false;
            this.isAddDirty = false;
            this.isDeleteDirty = false;
            ShownTransactions = null;
            LedgerEntry = null;
            this.entryLine = null;
            NewTransactionAmount = 0;
            NewTransactionIsCredit = false;
            NewTransactionIsDebit = true;
            NewTransactionIsReversal = false;
            NewTransactionNarrative = null;
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

            NewTransactionAmount = 0;
            NewTransactionIsCredit = false;
            NewTransactionIsDebit = false;
            NewTransactionIsReversal = false;
            NewTransactionNarrative = null;

            RaisePropertyChanged(() => LedgerEntry);
        }

        private void SaveBalanceAdjustment()
        {
            this.entryLine.BalanceAdjustment(NewTransactionAmount, NewTransactionNarrative);
            ShownTransactions = this.entryLine.BankBalanceAdjustments.ToList();
        }

        private void SaveNewEntryTransaction()
        {
            if (NewTransactionIsCredit || NewTransactionIsDebit)
            {
                LedgerTransaction newTransaction;
                if (NewTransactionIsCredit)
                {
                    newTransaction = new CreditLedgerTransaction();
                }
                else
                {
                    newTransaction = new DebitLedgerTransaction();
                }
                if (NewTransactionIsReversal)
                {
                    newTransaction.WithReversal(NewTransactionAmount).WithNarrative(NewTransactionNarrative);
                }
                else
                {
                    newTransaction.WithAmount(NewTransactionAmount).WithNarrative(NewTransactionNarrative);
                }

                LedgerEntry.AddTransaction(newTransaction);
                ShownTransactions = LedgerEntry.Transactions.ToList();
            }
        }

        private void ShowDialogCommon(bool isNew)
        {
            IsReadOnly = !isNew;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.Ok)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = Title,
            };
            MessengerInstance.Send(dialogRequest);
        }
    }
}