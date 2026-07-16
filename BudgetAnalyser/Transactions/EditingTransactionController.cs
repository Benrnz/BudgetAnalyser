using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public class EditingTransactionController : ControllerBase
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly TransactionsControllerFileOperations fileOperations;
    private readonly ILogger logger;
    private readonly ITransactionManagerService transactionService;
    private readonly IUserQuestionBoxYesNo yesNoBox;
    private BudgetBucket? originalBucket;

    public EditingTransactionController(
        IMessenger messenger,
        IBudgetBucketRepository bucketRepo,
        UserPrompts userPrompts,
        ITransactionManagerService transactionService,
        TransactionsControllerFileOperations fileOperations,
        ILogger logger)
        : base(messenger)
    {
        this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        this.fileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.yesNoBox = userPrompts?.YesNoBox ?? throw new ArgumentNullException(nameof(userPrompts));
        DeleteTransactionCommand = new RelayCommand<Transaction>(OnDeleteTransactionCommandExecute, t => t is not null);
    }

    public IEnumerable<BudgetBucket> Buckets
    {
        [UsedImplicitly]
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Array.Empty<BudgetBucket>();

    public IRelayCommand DeleteTransactionCommand { get; }

    public bool HasChanged => Transaction is not null && (OriginalHash != Transaction.GetEqualityHashCode() || this.originalBucket != Transaction.BudgetBucket);

    public int OriginalHash { get; private set; }

    public Transaction? Transaction
    {
        get;
        set
        {
            OriginalHash = value?.GetEqualityHashCode() ?? 0;
            field = value;
        }
    }

    public void ShowDialog(Transaction transaction, Guid correlationId)
    {
        Transaction = transaction;
        this.originalBucket = Transaction.BudgetBucket;
        Buckets = this.bucketRepo.Buckets.Where(b => b.Active);

        Messenger.Send(
            new ShellDialogRequestMessage(
                BudgetAnalyserFeature.Transactions,
                this,
                ShellDialogType.SaveCancel)
            {
                CorrelationId = correlationId,
                Title = "Edit Transaction"
            });
    }

    private void OnDeleteTransactionCommandExecute(Transaction? parameter = null)
    {
        if (parameter is null)
        {
            return;
        }

        var confirm = this.yesNoBox.Show("Are you sure you want to delete this transaction?", "Delete Transaction");
        if (confirm is not null && confirm.Value)
        {
            this.transactionService.RemoveTransaction(parameter);
            this.fileOperations.NotifyOfEdit();
            ObserveUnhandledFireAndForgetFailure(
                this.fileOperations.SyncWithServiceAsync(),
                ex => this.logger.LogError(ex, _ => "Unhandled exception processing DeleteTransactionCommandExecute."));
        }
    }
}
