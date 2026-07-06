using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     A controller for editing transactions and balance adjustments.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class LedgerTransactionsController : ControllerBase
{
    private readonly ILedgerService ledgerService;
    private readonly IReconciliationService reconService;
    private Guid dialogCorrelationId = Guid.NewGuid();
    private LedgerEntryLine? entryLine;
    private bool isAddDirty;
    private bool wasChanged;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
    public LedgerTransactionsController(IMessenger messenger, ILedgerService ledgerService, IReconciliationService reconService) : base(messenger)
    {
        this.ledgerService = ledgerService ?? throw new ArgumentNullException(nameof(ledgerService));
        this.reconService = reconService ?? throw new ArgumentNullException(nameof(reconService));
        Messenger.Register<LedgerTransactionsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        AddBalanceAdjustmentCommand = new RelayCommand(OnAddNewAdjustmentCommandExecuted, () => InBalanceAdjustmentMode && !IsReadOnly);
        DeleteTransactionCommand = new RelayCommand<LedgerTransaction?>(OnDeleteTransactionCommandExecuted, CanExecuteDeleteTransactionCommand);
        Reset();
    }

    public IEnumerable<Account> Accounts => this.ledgerService.ValidLedgerAccounts();

    public IRelayCommand AddBalanceAdjustmentCommand { get; }

    [UsedImplicitly]
    public IRelayCommand<LedgerTransaction?> DeleteTransactionCommand { get; }

    public bool InBalanceAdjustmentMode
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            AddBalanceAdjustmentCommand.NotifyCanExecuteChanged();
        }
    }

    public bool InLedgerEntryMode
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsReadOnly
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            AddBalanceAdjustmentCommand.NotifyCanExecuteChanged();
            DeleteTransactionCommand.NotifyCanExecuteChanged();
        }
    }

    public LedgerEntry? LedgerEntry
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InBalanceAdjustmentMode));
            OnPropertyChanged(nameof(InLedgerEntryMode));
        }
    }

    public Account? NewTransactionAccount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal NewTransactionAmount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string? NewTransactionNarrative
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal OpeningBalance { get; private set; }

    public bool ShowAddingNewTransactionPanel
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LedgerTransaction> ShownTransactions { get; private set; } = new();

    public string Title
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public decimal TransactionsTotal => ShownTransactions.Sum(t => t.Amount);

    /// <summary>
    ///     Show the Ledger Transactions view, for viewing and editing Balance Adjustments
    /// </summary>
    public void ShowBankBalanceAdjustmentsDialog(LedgerEntryLine ledgerEntryLine, bool isReadOnly)
    {
        this.entryLine = ledgerEntryLine ?? throw new ArgumentNullException(nameof(ledgerEntryLine));
        InBalanceAdjustmentMode = true;
        InLedgerEntryMode = false;
        LedgerEntry = null;
        ShownTransactions = new ObservableCollection<LedgerTransaction>(ledgerEntryLine.BankBalanceAdjustments);
        Title = "Balance Adjustment Transactions";
        ShowDialogCommon(isReadOnly);
    }

    /// <summary>
    ///     Show the Ledger Transactions view for viewing and editing Ledger Transactions.
    /// </summary>
    public void ShowLedgerTransactionsDialog(LedgerEntryLine ledgerEntryLine, LedgerEntry ledgerEntry)
    {
        LedgerEntry = ledgerEntry ?? throw new ArgumentNullException(nameof(ledgerEntry));
        InBalanceAdjustmentMode = false;
        InLedgerEntryMode = true;
        this.entryLine = ledgerEntryLine; // Will be null when editing an existing LedgerEntry as opposed to creating a new reconciliation.
        ShownTransactions.Clear();
        LedgerEntry.Transactions.ToList().ForEach(t => ShownTransactions.Add(t));
        Title = $"{ledgerEntry.LedgerBucket.BudgetBucket.Code} Transactions";
        OpeningBalance = RetrieveOpeningBalance();
        ShowDialogCommon(true);  // Ledger Transactions cannot be edited or deleted.
    }

    private bool CanExecuteDeleteTransactionCommand(LedgerTransaction? arg)
    {
        return !IsReadOnly && arg is not null;
    }

    private void OnAddNewAdjustmentCommandExecuted()
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

    private void OnDeleteTransactionCommandExecuted(LedgerTransaction? transaction)
    {
        if (IsReadOnly)
        {
            return;
        }

        if (transaction is null || this.ledgerService.LedgerBook is null)
        {
            return;
        }

        if (InBalanceAdjustmentMode)
        {
            this.wasChanged = true;
            this.reconService.CancelBalanceAdjustment(this.entryLine!, transaction.Id);
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
                OnAddNewAdjustmentCommandExecuted();
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

        Messenger.Send(new LedgerTransactionsCompletedMessage(this.wasChanged));

        Reset();
    }

    private void Reset()
    {
        this.wasChanged = false;
        ShowAddingNewTransactionPanel = false;
        this.isAddDirty = false;
        NewTransactionAmount = 0;
        NewTransactionNarrative = null;
        NewTransactionAccount = null;
    }

    private decimal RetrieveOpeningBalance()
    {
        var book = this.ledgerService.LedgerBook ?? throw new InvalidOperationException("LedgerBook is null and not initialised.");
        var found = false;
        var remainingRecons = book.Reconciliations.SkipWhile(r =>
        {
            // Find the recon that directly precedes this current one.
            if (found)
            {
                return false; // Found recon line on previous pass, now return.
            }

            found = r.Entries.Contains(LedgerEntry);
            return true; // Keep skipping...
        }).Take(1);

        var previousLine = remainingRecons.FirstOrDefault();
        if (previousLine is null)
        {
            return 0M;
        }

        var previousEntry = previousLine.Entries.FirstOrDefault(l => l.LedgerBucket == LedgerEntry!.LedgerBucket);
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
            var newTransaction = this.reconService.CreateBalanceAdjustment(this.entryLine!, NewTransactionAmount, NewTransactionNarrative ?? string.Empty, NewTransactionAccount!);
            ShownTransactions.Add(newTransaction);
            this.wasChanged = true;
            OnPropertyChanged(nameof(TransactionsTotal));
        }

        Reset();
        OnPropertyChanged(nameof(LedgerEntry));
    }

    private void ShowDialogCommon(bool isReadOnly)
    {
        IsReadOnly = isReadOnly;
        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, IsReadOnly ? ShellDialogType.Close : ShellDialogType.OkCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = Title
        };
        Messenger.Send(dialogRequest);
    }
}
