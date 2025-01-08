using System.Collections.ObjectModel;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC(SingleInstance = true)]
public class LedgerBucketViewController : ControllerBase
{
    private readonly IAccountTypeRepository accountRepo;
    private readonly ILedgerService ledgerService;
    private readonly IUserMessageBox messageBox;
    private Guid correlationId = Guid.NewGuid();
    private LedgerBucket? ledger;

    public LedgerBucketViewController(IAccountTypeRepository accountRepo, IUiContext uiContext, ILedgerService ledgerService) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
        this.ledgerService = ledgerService ?? throw new ArgumentNullException(nameof(ledgerService));

        this.messageBox = uiContext.UserPrompts.MessageBox;
        Messenger.Register<LedgerBucketViewController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public event EventHandler? Updated;

    public ObservableCollection<Account> BankAccounts { get; } = new();

    public BudgetBucket? BucketBeingTracked { get; private set; }

    public decimal BudgetAmount { get; private set; }

    public BudgetCycle BudgetCycle { get; private set; }

    public Account? StoredInAccount { get; set; }

    public void ShowDialog(LedgerBucket ledgerBucket, BudgetModel budgetModel)
    {
        if (budgetModel is null)
        {
            throw new ArgumentNullException(nameof(budgetModel));
        }

        this.ledger = ledgerBucket ?? throw new ArgumentNullException(nameof(ledgerBucket));

        this.correlationId = Guid.NewGuid();

        BankAccounts.Clear();
        this.accountRepo.ListCurrentlyUsedAccountTypes().ToList().ForEach(a => BankAccounts.Add(a));
        BucketBeingTracked = ledgerBucket.BudgetBucket;
        StoredInAccount = ledgerBucket.StoredInAccount;
        BudgetAmount = budgetModel.Expenses.Single(e => e.Bucket == BucketBeingTracked).Amount;
        BudgetCycle = budgetModel.BudgetCycle;

        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
        {
            CorrelationId = this.correlationId, Title = "Ledger - " + BucketBeingTracked, HelpAvailable = true
        };

        Messenger.Send(dialogRequest);
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.correlationId))
        {
            return;
        }

        if (message.Response == ShellDialogButton.Cancel)
        {
            return;
        }

        if (message.Response == ShellDialogButton.Help)
        {
            this.messageBox.Show(
                "Ledgers within the Ledger Book track the actual bank balance over time of a single Bucket.  This is especially useful for budget items that you need to save up for. For example, annual vehicle registration, or car maintenance.  It can also be useful to track Spent-Monthly/Fortnightly Buckets. Even though they are always spent down to zero each month, (like rent or mortgage payments), sometimes its useful to have an extra payment, for when there are five weekly payments in a month instead of four.");
            return;
        }

        try
        {
            if (this.ledger!.StoredInAccount == StoredInAccount!)
            {
                return;
            }

            this.ledgerService.MoveLedgerToAccount(this.ledger, StoredInAccount!);
            var handler = Updated;
            handler?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            Reset();
        }
    }

    private void Reset()
    {
        this.ledger = null;
        BudgetAmount = 0;
        BankAccounts.Clear();
        StoredInAccount = null;
    }
}
