using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerTransactionsController : ControllerBase, IShowableController
    {
        private bool doNotUseAddingNewTransaction;
        private bool doNotUseIsReadOnly;
        private LedgerEntry doNotUseLedgerEntry;
        private decimal doNotUseNewTransactionAmount;
        private bool doNotUseNewTransactionIsCredit;
        private bool doNotUseNewTransactionIsDebit;
        private bool doNotUseNewTransactionIsReversal;
        private string doNotUseNewTransactionNarrative;
        private bool doNotUseShown;
        private IEnumerable<LedgerTransaction> doNotUseShownTransactions;
        private string doNotUseTitle;
        private LedgerEntryLine entryLine;
        private bool isAddDirty;
        private bool isDeleteDirty;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LedgerTransactionsController()
        {
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

        public ICommand CloseTransactionsViewCommand
        {
            get { return new RelayCommand(OnCloseTransactionsViewCommandExecuted); }
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

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
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
            get { return new RelayCommand(OnZeroNetAmountCommandExecuted); }
        }

        public void Show(LedgerEntry ledgerEntry, bool isNew)
        {
            if (ledgerEntry == null)
            {
                return;
            }

            LedgerEntry = ledgerEntry;
            ShownTransactions = LedgerEntry.Transactions;
            Title = "Ledger Entry Transactions";
            IsReadOnly = !isNew;
            Shown = true;
        }

        public void Show(LedgerEntryLine ledgerEntryLine, bool isNew)
        {
            if (ledgerEntryLine == null)
            {
                return;
            }

            LedgerEntry = null;
            ShownTransactions = ledgerEntryLine.BankBalanceAdjustments;
            Title = "Balance Adjustment Transactions";
            IsReadOnly = !isNew;
            this.entryLine = ledgerEntryLine;
            Shown = true;
        }

        private bool CanExecuteAddTransactionCommand()
        {
            return !IsReadOnly;
        }

        private bool CanExecuteDeleteTransactionCommand(LedgerTransaction arg)
        {
            return !IsReadOnly && arg != null;
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

        private void OnCloseTransactionsViewCommandExecuted()
        {
            Shown = false;
            Save();

            EventHandler<LedgerTransactionEventArgs> handler = Complete;
            if (handler != null)
            {
                handler(this, new LedgerTransactionEventArgs(this.isAddDirty || this.isDeleteDirty));
            }

            Reset();
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

        private void OnZeroNetAmountCommandExecuted()
        {
            if (LedgerEntry == null)
            {
                return;
            }

            NewTransactionIsDebit = true;
            NewTransactionIsCredit = false;
            NewTransactionAmount = LedgerEntry.NetAmount;
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
    }
}