using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC(SingleInstance = true)]
public class AddLedgerReconciliationController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
{
    private readonly IAccountTypeRepository accountTypeRepository;
    private readonly IUserMessageBox messageBox;
    private Guid dialogCorrelationId;
    private Engine.Ledger.LedgerBook? parentBook;

    public AddLedgerReconciliationController(IMessenger messenger, UserPrompts userPrompts, IAccountTypeRepository accountTypeRepository) : base(messenger)
    {
        this.accountTypeRepository = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));
        if (accountTypeRepository is null)
        {
            throw new ArgumentNullException(nameof(accountTypeRepository));
        }

        Messenger.Register<AddLedgerReconciliationController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        this.messageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
    }

    // TODO Change this event to a message:
    public event EventHandler<EditBankBalancesEventArgs>? Complete;

    public bool AddBalanceVisibility
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

    public ICommand AddBankBalanceCommand => new RelayCommand(OnAddBankBalanceCommandExecuted, CanExecuteAddBankBalanceCommand);

    public decimal? AdjustedBankBalanceTotal => AddBalanceVisibility ? default(decimal?) : BankBalances.Sum(b => b.AdjustedBalance);

    public IEnumerable<Account> BankAccounts
    {
        get;
        private set
        {
            if (ReferenceEquals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = Array.Empty<Account>();

    public decimal BankBalance
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BankBalanceTotal));
            OnPropertyChanged(nameof(AdjustedBankBalanceTotal));
        }
    }

    public ObservableCollection<BankBalanceViewModel> BankBalances { get; private set; } = new();

    public decimal BankBalanceTotal => BankBalances.Sum(b => b.Balance);
    public bool Canceled { get; private set; }
    public bool CreateMode { get; private set; }

    public DateOnly Date
    {
        get;
        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public bool Editable
    {
        get;
        private set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    /// <summary>
    ///     Checks to make sure the <see cref="BankBalances" /> collection contains a balance for every ledger that will be
    ///     included in the reconciliation.
    /// </summary>
    public bool HasRequiredBalances => this.parentBook is not null && this.parentBook.Ledgers.All(l => BankBalances.Any(b => b.Account == l.StoredInAccount));

    [UsedImplicitly]
    public ICommand RemoveBankBalanceCommand => new RelayCommand<BankBalanceViewModel>(OnRemoveBankBalanceCommandExecuted, _ => Editable);

    public Account? SelectedBankAccount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool CanExecuteCancelButton => true;

    public bool CanExecuteOkButton => CreateMode ? Date != DateOnly.MinValue && HasRequiredBalances : Editable && Date != DateOnly.MinValue && HasRequiredBalances;

    public bool CanExecuteSaveButton => false;

    public string ActionButtonToolTip => "Add new ledger entry line.";
    public string CloseButtonToolTip => "Cancel";

    /// <summary>
    ///     Used to start a new Ledger Book reconciliation.  This will ultimately add a new <see cref="LedgerEntryLine" /> to the <see cref="LedgerBook" />.
    /// </summary>
    public void ShowCreateDialog(Engine.Ledger.LedgerBook ledgerBook)
    {
        this.parentBook = ledgerBook ?? throw new ArgumentNullException(nameof(ledgerBook));
        BankBalances = new ObservableCollection<BankBalanceViewModel>();
        CreateMode = true;
        AddBalanceVisibility = true;
        Editable = true;

        var requestFilterMessage = new RequestFilterMessage(this);
        Messenger.Send(requestFilterMessage);
        if (requestFilterMessage.Criteria is null)
        {
            return;
        }

        Date = requestFilterMessage.Criteria.EndDate?.AddDays(1) ?? DateOnlyExt.Today();

        ShowDialogCommon("Closing Period Reconciliation");
    }

    /// <summary>
    ///     Used to show the bank balances involved in this <see cref="LedgerEntryLine" />.
    ///     Only shows balances at this stage, no editing allowed.
    /// </summary>
    public void ShowViewDialog(Engine.Ledger.LedgerBook ledgerBook, LedgerEntryLine line)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        this.parentBook = ledgerBook ?? throw new ArgumentNullException(nameof(ledgerBook));
        Date = line.Date;
        BankBalances = new ObservableCollection<BankBalanceViewModel>(line.BankBalances.Select(b => new BankBalanceViewModel(line, b)));
        CreateMode = false;
        AddBalanceVisibility = false;
        Editable = false; // Bank balances are not editable after creating a new Ledger Line at this stage.

        ShowDialogCommon(Editable ? "Edit Bank Balances" : "Bank Balances");
    }

    private void AddNewBankBalance()
    {
        if (SelectedBankAccount is null)
        {
            return;
        }

        BankBalances.Add(new BankBalanceViewModel(null, SelectedBankAccount, BankBalance));
        SelectedBankAccount = null;
        BankBalance = 0;
        OnPropertyChanged(nameof(HasRequiredBalances));
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    private bool CanExecuteAddBankBalanceCommand()
    {
        if (CreateMode)
        {
            return SelectedBankAccount is not null;
        }

        if (!Editable)
        {
            return false;
        }

        return !AddBalanceVisibility || SelectedBankAccount is not null;
    }

    private void OnAddBankBalanceCommandExecuted()
    {
        if (CreateMode)
        {
            AddNewBankBalance();
            return;
        }

        if (!AddBalanceVisibility)
        {
            AddBalanceVisibility = true;
            return;
        }

        AddBalanceVisibility = false;
        AddNewBankBalance();
    }

    private void OnRemoveBankBalanceCommandExecuted(BankBalanceViewModel? bankBalance)
    {
        if (bankBalance is null)
        {
            return;
        }

        BankBalances.Remove(bankBalance);
        OnPropertyChanged(nameof(BankBalanceTotal));
        OnPropertyChanged(nameof(AdjustedBankBalanceTotal));
        OnPropertyChanged(nameof(HasRequiredBalances));
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (message.Response == ShellDialogButton.Help)
        {
            this.messageBox.Show(
                "Use your actual pay day as the date, this signifies the closing of the previous period, and opening a new period on your pay day showing available funds in each ledger for the new " +
                "month/fortnight. The date used will also select bank transactions from the transactions list to calculate the ledger balance. The date is set from the current date range filter (on" +
                " the dashboard page), using the day after the end date. Bank Transactions will be selected by searching one month/fortnight prior to the given date, excluding this given date. The " +
                "transactions are used to show how the current ledger balance is calculated. The bank balance is the balance as at the date given, after your pay has been credited.");
            return;
        }

        try
        {
            if (message.Response == ShellDialogButton.Cancel)
            {
                Canceled = true;
            }
            else
            {
                if (BankBalances.None())
                {
                    if (CanExecuteAddBankBalanceCommand())
                    {
                        // User may have entered some data to add a new bank balance to the collection, but not clicked the add button, instead they just clicked save.
                        OnAddBankBalanceCommandExecuted();
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to add any bank balances to Ledger Book reconciliation.");
                    }
                }
            }

            var handler = Complete;
            handler?.Invoke(this, new EditBankBalancesEventArgs { Canceled = Canceled });
        }
        finally
        {
            Reset();
        }
    }

    private void Reset()
    {
        this.parentBook = null;
        Date = DateOnly.MinValue;
        BankBalances = new ObservableCollection<BankBalanceViewModel>();
        BankAccounts = Array.Empty<Account>();
        SelectedBankAccount = null;
    }

    private void ShowDialogCommon(string title)
    {
        Canceled = false;
        var accountsToShow = this.accountTypeRepository.ListCurrentlyUsedAccountTypes().ToList();
        BankAccounts = accountsToShow.OrderBy(a => a.Name);
        SelectedBankAccount = null;
        if (CreateMode)
        {
            SelectedBankAccount = BankAccounts.FirstOrDefault(account => account.IsSalaryAccount);
        }

        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
        {
            CorrelationId = this.dialogCorrelationId, Title = title, HelpAvailable = CreateMode
        };

        Messenger.Send(dialogRequest);
    }
}
