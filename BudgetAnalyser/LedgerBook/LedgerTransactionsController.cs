using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     A controller for editing transactions and balance adjustments.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerTransactionsController : ControllerBase
    {
        private readonly ILedgerService ledgerService;
        private readonly IReconciliationService reconService;
        private Guid dialogCorrelationId;
        private bool doNotUseIsReadOnly;
        private LedgerEntry doNotUseLedgerEntry;
        private Account doNotUseNewTransactionAccount;
        private decimal doNotUseNewTransactionAmount;
        private string doNotUseNewTransactionNarrative;
        private bool doNotUseShowAddingNewTransactionPanel;
        private string doNotUseTitle;
        private LedgerEntryLine entryLine;
        private bool isAddDirty;
        private bool wasChanged;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LedgerTransactionsController([NotNull] UiContext uiContext, [NotNull] ILedgerService ledgerService, [NotNull] IReconciliationService reconService) : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (ledgerService == null)
            {
                throw new ArgumentNullException(nameof(ledgerService));
            }

            if (reconService == null)
            {
                throw new ArgumentNullException(nameof(reconService));
            }

            this.ledgerService = ledgerService;
            this.reconService = reconService;
            Messenger.Register<LedgerTransactionsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
            Reset();
        }

        [UsedImplicitly]
        public IEnumerable<Account> Accounts => this.ledgerService.ValidLedgerAccounts();

        [UsedImplicitly]
        public ICommand AddBalanceAdjustmentCommand => new RelayCommand(OnAddNewTransactionCommandExecuted, () => IsAddBalanceAdjustmentAllowed && !IsReadOnly);

        [UsedImplicitly]
        public ICommand DeleteTransactionCommand => new RelayCommand<LedgerTransaction>(OnDeleteTransactionCommandExecuted, CanExecuteDeleteTransactionCommand);

        public bool InBalanceAdjustmentMode { get; private set; }
        public bool InLedgerEntryMode { get; private set; }

        public bool IsAddBalanceAdjustmentAllowed => InBalanceAdjustmentMode;

        public bool IsReadOnly
        {
            get { return this.doNotUseIsReadOnly; }
            set
            {
                this.doNotUseIsReadOnly = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAddBalanceAdjustmentAllowed));
            }
        }

        public LedgerEntry LedgerEntry
        {
            get { return this.doNotUseLedgerEntry; }
            private set
            {
                this.doNotUseLedgerEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InBalanceAdjustmentMode));
                OnPropertyChanged(nameof(InLedgerEntryMode));
            }
        }

        public Account NewTransactionAccount
        {
            get { return this.doNotUseNewTransactionAccount; }
            set
            {
                this.doNotUseNewTransactionAccount = value;
                OnPropertyChanged();
            }
        }

        public decimal NewTransactionAmount
        {
            get { return this.doNotUseNewTransactionAmount; }
            set
            {
                this.doNotUseNewTransactionAmount = value;
                OnPropertyChanged();
            }
        }

        public string NewTransactionNarrative
        {
            get { return this.doNotUseNewTransactionNarrative; }
            set
            {
                this.doNotUseNewTransactionNarrative = value;
                OnPropertyChanged();
            }
        }

        public decimal OpeningBalance { get; private set; }

        public bool ShowAddingNewTransactionPanel
        {
            get { return this.doNotUseShowAddingNewTransactionPanel; }
            private set
            {
                this.doNotUseShowAddingNewTransactionPanel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LedgerTransaction> ShownTransactions { get; private set; }

        public string Title
        {
            get { return this.doNotUseTitle; }
            private set
            {
                this.doNotUseTitle = value;
                OnPropertyChanged();
            }
        }

        public decimal TransactionsTotal => ShownTransactions?.Sum(t => t.Amount) ?? 0;

        [UsedImplicitly]
        public ICommand ZeroNetAmountCommand => new RelayCommand(OnZeroNetAmountCommandExecuted, CanExecuteZeroNetAmountCommand);

        public event EventHandler<LedgerTransactionEventArgs> Complete;

        /// <summary>
        ///     Show the Ledger Transactions view for viewing and editing Ledger Transactions.
        /// </summary>
        public void ShowLedgerTransactionsDialog(LedgerEntryLine ledgerEntryLine, LedgerEntry ledgerEntry, bool isNew)
        {
            if (ledgerEntry == null)
            {
                return;
            }

            InBalanceAdjustmentMode = false;
            InLedgerEntryMode = true;
            LedgerEntry = ledgerEntry;
            this.entryLine = ledgerEntryLine; // Will be null when editing an existing LedgerEntry as opposed to creating a new reconciliation.
            ShownTransactions = new ObservableCollection<LedgerTransaction>(LedgerEntry.Transactions);
            Title = string.Format(CultureInfo.CurrentCulture, "{0} Transactions", ledgerEntry.LedgerBucket.BudgetBucket.Code);
            OpeningBalance = RetrieveOpeningBalance();
            ShowDialogCommon(isNew);
        }

        /// <summary>
        ///     Show the Ledger Transactions view, for viewing and editing Balance Adjustments
        /// </summary>
        public void ShowBankBalanceAdjustmentsDialog(LedgerEntryLine ledgerEntryLine, bool isNew)
        {
            if (ledgerEntryLine == null)
            {
                return;
            }

            InBalanceAdjustmentMode = true;
            InLedgerEntryMode = false;
            LedgerEntry = null;
            this.entryLine = ledgerEntryLine;
            ShownTransactions = new ObservableCollection<LedgerTransaction>(ledgerEntryLine.BankBalanceAdjustments);
            Title = "Balance Adjustment Transactions";
            ShowDialogCommon(isNew);
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
                this.reconService.RemoveTransaction(this.ledgerService.LedgerBook, LedgerEntry, transaction.Id);
                ShownTransactions.Remove(transaction);
            }
            else if (InBalanceAdjustmentMode)
            {
                this.wasChanged = true;
                this.reconService.CancelBalanceAdjustment(this.entryLine, transaction.Id);
                ShownTransactions.Remove(transaction);
            }

            OnPropertyChanged(nameof(TransactionsTotal));
            OnPropertyChanged(nameof(LedgerEntry));
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
            handler?.Invoke(this, new LedgerTransactionEventArgs(this.wasChanged));

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
            NewTransactionAccount = null;
        }

        private decimal RetrieveOpeningBalance()
        {
            var book = this.ledgerService.LedgerBook;
            bool found = false;
            IEnumerable<LedgerEntryLine> remainingRecons = book.Reconciliations.SkipWhile(r =>
            {
                // Find the recon that directly precedes this current one.
                if (found) return false; // Found recon line on previous pass, now return.
                found = r.Entries.Contains(LedgerEntry);
                return true; // Keep skipping...
            }).Take(1);

            LedgerEntryLine previousLine = remainingRecons.FirstOrDefault();
            if (previousLine == null) return 0M;
            var previousEntry = previousLine.Entries.FirstOrDefault(l => l.LedgerBucket == LedgerEntry.LedgerBucket);
            return previousEntry?.Balance ?? 0M;
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
            OnPropertyChanged(nameof(LedgerEntry));
        }

        private void SaveBalanceAdjustment()
        {
            LedgerTransaction newTransaction = this.reconService.CreateBalanceAdjustment(this.entryLine, NewTransactionAmount, NewTransactionNarrative, NewTransactionAccount);
            ShownTransactions.Add(newTransaction);
            this.wasChanged = true;
            OnPropertyChanged(nameof(TransactionsTotal));
        }

        private void SaveNewEntryTransaction()
        {
            try
            {
                Debug.Assert(this.entryLine != null);
                var newTransaction = this.reconService.CreateLedgerTransaction(this.ledgerService.LedgerBook, this.entryLine, LedgerEntry, NewTransactionAmount, NewTransactionNarrative);
                ShownTransactions.Add(newTransaction);
            }
            catch (ArgumentException)
            {
                // Invalid transaction data
                return;
            }

            OnPropertyChanged(nameof(TransactionsTotal));
            this.wasChanged = true;
        }

        private void ShowDialogCommon(bool isNew)
        {
            IsReadOnly = !isNew;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, IsReadOnly ? ShellDialogType.Ok : ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = Title
            };
            Messenger.Send(dialogRequest);
        }
    }
}