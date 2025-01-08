using System.Collections.ObjectModel;
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
    private bool doNotUseIsReadOnly;
    private LedgerEntry? doNotUseLedgerEntry;
    private Account? doNotUseNewTransactionAccount;
    private decimal doNotUseNewTransactionAmount;
    private string? doNotUseNewTransactionNarrative;
    private bool doNotUseShowAddingNewTransactionPanel;
    private string doNotUseTitle = string.Empty;
    private LedgerEntryLine? entryLine;
    private bool isAddDirty;
    private bool wasChanged;
    private readonly ILogger logger;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
    public LedgerTransactionsController(UiContext uiContext, ILedgerService ledgerService, IReconciliationService reconService) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.ledgerService = ledgerService ?? throw new ArgumentNullException(nameof(ledgerService));
        this.reconService = reconService ?? throw new ArgumentNullException(nameof(reconService));
        this.logger = uiContext.Logger;
        Messenger.Register<LedgerTransactionsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        Reset();
    }

    public event EventHandler<LedgerTransactionEventArgs>? Complete;

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
        get => this.doNotUseIsReadOnly;
        set
        {
            this.doNotUseIsReadOnly = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAddBalanceAdjustmentAllowed));
        }
    }

    public LedgerEntry? LedgerEntry
    {
        get => this.doNotUseLedgerEntry;
        private set
        {
            this.doNotUseLedgerEntry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InBalanceAdjustmentMode));
            OnPropertyChanged(nameof(InLedgerEntryMode));
        }
    }

    public Account? NewTransactionAccount
    {
        get => this.doNotUseNewTransactionAccount;
        set
        {
            this.doNotUseNewTransactionAccount = value;
            OnPropertyChanged();
        }
    }

    public decimal NewTransactionAmount
    {
        get => this.doNotUseNewTransactionAmount;
        set
        {
            this.doNotUseNewTransactionAmount = value;
            OnPropertyChanged();
        }
    }

    public string? NewTransactionNarrative
    {
        get => this.doNotUseNewTransactionNarrative;
        set
        {
            this.doNotUseNewTransactionNarrative = value;
            OnPropertyChanged();
        }
    }

    public decimal OpeningBalance { get; private set; }

    public bool ShowAddingNewTransactionPanel
    {
        get => this.doNotUseShowAddingNewTransactionPanel;
        private set
        {
            this.doNotUseShowAddingNewTransactionPanel = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LedgerTransaction> ShownTransactions { get; private set; } = new();

    public string Title
    {
        get => this.doNotUseTitle;
        private set
        {
            this.doNotUseTitle = value;
            OnPropertyChanged();
        }
    }

    public decimal TransactionsTotal => ShownTransactions.Sum(t => t.Amount);

    [UsedImplicitly]
    public ICommand ZeroNetAmountCommand => new RelayCommand(OnZeroNetAmountCommandExecuted, CanExecuteZeroNetAmountCommand);

    /// <summary>
    ///     Show the Ledger Transactions view, for viewing and editing Balance Adjustments
    /// </summary>
    public void ShowBankBalanceAdjustmentsDialog(LedgerEntryLine ledgerEntryLine, bool isNew)
    {
        this.entryLine = ledgerEntryLine ?? throw new ArgumentNullException(nameof(ledgerEntryLine));
        InBalanceAdjustmentMode = true;
        InLedgerEntryMode = false;
        LedgerEntry = null;
        ShownTransactions = new ObservableCollection<LedgerTransaction>(ledgerEntryLine.BankBalanceAdjustments);
        Title = "Balance Adjustment Transactions";
        ShowDialogCommon(isNew);
    }

    /// <summary>
    ///     Show the Ledger Transactions view for viewing and editing Ledger Transactions.
    /// </summary>
    public void ShowLedgerTransactionsDialog(LedgerEntryLine ledgerEntryLine, LedgerEntry ledgerEntry, bool isNew)
    {
        LedgerEntry = ledgerEntry ?? throw new ArgumentNullException(nameof(ledgerEntry));
        InBalanceAdjustmentMode = false;
        InLedgerEntryMode = true;
        this.entryLine = ledgerEntryLine; // Will be null when editing an existing LedgerEntry as opposed to creating a new reconciliation.
        ShownTransactions.Clear();
        LedgerEntry.Transactions.ToList().ForEach(t => ShownTransactions.Add(t));
        Title = string.Format(CultureInfo.CurrentCulture, "{0} Transactions", ledgerEntry.LedgerBucket.BudgetBucket.Code);
        OpeningBalance = RetrieveOpeningBalance();
        ShowDialogCommon(isNew);
    }

    private bool CanExecuteDeleteTransactionCommand(LedgerTransaction? arg)
    {
        return !IsReadOnly && arg is not null;
    }

    private bool CanExecuteZeroNetAmountCommand()
    {
        return LedgerEntry is not null && LedgerEntry.NetAmount != 0;
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

    private void OnDeleteTransactionCommandExecuted(LedgerTransaction? transaction)
    {
        if (IsReadOnly)
        {
            return;
        }

        if (transaction is null)
        {
            return;
        }

        if (InLedgerEntryMode)
        {
            this.wasChanged = true;
            this.reconService.RemoveTransaction(this.ledgerService.LedgerBook, LedgerEntry!, transaction.Id);
            ShownTransactions.Remove(transaction);
        }
        else if (InBalanceAdjustmentMode)
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

        var handler = Complete;
        handler?.Invoke(this, new LedgerTransactionEventArgs(this.wasChanged));

        Reset();
        this.wasChanged = false;
    }

    private void OnZeroNetAmountCommandExecuted()
    {
        if (LedgerEntry is null)
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
        var newTransaction = this.reconService.CreateBalanceAdjustment(this.entryLine!, NewTransactionAmount, NewTransactionNarrative ?? string.Empty, NewTransactionAccount!);
        ShownTransactions.Add(newTransaction);
        this.wasChanged = true;
        OnPropertyChanged(nameof(TransactionsTotal));
    }

    private void SaveNewEntryTransaction()
    {
        try
        {
            if (this.entryLine is null || LedgerEntry is null)
            {
                this.logger.LogError(
                    l => l.Format(
                                    "Silent error: Attempt to create a new ledger transaction, but LedgerLine or LedgerEntry is null. LedgerLine: {0}, LedgerEntry: {1}",
                                    this.entryLine,
                                    LedgerEntry));
                return;
            }

            var newTransaction = this.reconService.CreateLedgerTransaction(this.ledgerService.LedgerBook, this.entryLine, LedgerEntry, NewTransactionAmount, NewTransactionNarrative ?? string.Empty);
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
