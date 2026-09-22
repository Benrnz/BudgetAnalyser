using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public partial class SplitTransactionController : ControllerBase, IShellDialogInteractivity
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly ITransactionsControllerFileOperations fileOperations;
    private readonly ILogger logger;
    private readonly ITransactionManagerService transactionsService;
    private Guid dialogCorrelationId;

    public SplitTransactionController(
        IMessenger messenger,
        IBudgetBucketRepository bucketRepo,
        ILogger logger,
        ITransactionManagerService transactionsService,
        ITransactionsControllerFileOperations fileOperations) : base(messenger)
    {
        this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
        this.fileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        Messenger.Register<SplitTransactionController, ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        CalculateSplinter1Command = new RelayCommand(CalculateSplinter2);
        CalculateSplinter2Command = new RelayCommand(CalculateSplinter1);
    }

    public IEnumerable<BudgetBucket> BudgetBuckets { get; private set; } = [];

    public ICommand CalculateSplinter1Command { get; }
    public ICommand CalculateSplinter2Command { get; }

    [ObservableProperty]
    public partial string? InvalidMessage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Valid))]
    public partial Transaction? OriginalTransaction { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAmount))]
    [NotifyPropertyChangedFor(nameof(Valid))]
    public partial decimal SplinterAmount1 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalAmount))]
    [NotifyPropertyChangedFor(nameof(Valid))]
    public partial decimal SplinterAmount2 { get; set; }

    public BudgetBucket? SplinterBucket1 { get; set; }
    public BudgetBucket? SplinterBucket2 { get; set; }
    public decimal TotalAmount => SplinterAmount1 + SplinterAmount2;

    public bool Valid
    {
        get
        {
            if (OriginalTransaction is null)
            {
                return false;
            }

            if (SplinterAmount1 == 0)
            {
                InvalidMessage = "Amount 1 cannot be zero.";
                return false;
            }

            if (SplinterAmount2 == 0)
            {
                InvalidMessage = "Amount 2 cannot be zero.";
                return false;
            }

            if (Math.Abs(SplinterAmount1) + Math.Abs(SplinterAmount2) != Math.Abs(OriginalTransaction.Amount))
            {
                InvalidMessage = "Cannot mix debit and credit amounts.";
                return false;
            }

            if (decimal.Round(SplinterAmount1 + SplinterAmount2, 2) != decimal.Round(OriginalTransaction.Amount, 2))
            {
                InvalidMessage = $"The two amounts do not add up to {OriginalTransaction.Amount:C}";
                return false;
            }

            if (SplinterBucket1 == null || SplinterBucket2 == null)
            {
                InvalidMessage = "Both buckets must have a budget bucket selected.";
                return false;
            }

            return true;
        }
    }

    public bool CanExecuteCancelButton => true;
    public bool CanExecuteOkButton => false;
    public bool CanExecuteSaveButton => Valid;

    public void ShowDialog(Transaction originalTransaction)
    {
        BudgetBuckets = this.bucketRepo.Buckets;
        this.dialogCorrelationId = Guid.NewGuid();
        OriginalTransaction = originalTransaction;
        SplinterAmount1 = OriginalTransaction.Amount;
        SplinterAmount2 = 0M;
        SplinterBucket2 = SplinterBucket1 = originalTransaction.BudgetBucket;

        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.SaveCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Split Transaction"
        };
        Messenger.Send(dialogRequest);
    }

    private void CalculateSplinter1()
    {
        if (OriginalTransaction == null)
        {
            return;
        }

        var other = decimal.Round(SplinterAmount2, 2);
        SplinterAmount1 = decimal.Round(OriginalTransaction.Amount - other, 2);
    }

    private void CalculateSplinter2()
    {
        if (OriginalTransaction == null)
        {
            return;
        }

        var other = decimal.Round(SplinterAmount1, 2);
        SplinterAmount2 = decimal.Round(OriginalTransaction.Amount - other, 2);
    }

    private async Task FinaliseSplitTransaction(ShellDialogResponseMessage message)
    {
        if (message.Response == ShellDialogButton.Save && OriginalTransaction is not null)
        {
            this.transactionsService.SplitTransaction(
                OriginalTransaction,
                SplinterAmount1,
                SplinterAmount2,
                SplinterBucket1!,
                SplinterBucket2!); // Buckets already validated by CanExecute methods on the Commands.

            this.fileOperations.NotifyOfEdit();
            await this.fileOperations.SyncWithServiceAsync();
        }
    }

    partial void OnOriginalTransactionChanged(Transaction? value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    private void OnShellDialogResponseReceived(SplitTransactionController recipient, ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        ObserveUnhandledFireAndForgetFailure(
            FinaliseSplitTransaction(message),
            ex => this.logger.LogError(ex, _ => "Unhandled exception processing FinaliseSplitTransaction in SplitTransactionController."));

        this.dialogCorrelationId = Guid.Empty;
        OriginalTransaction = null;
    }

    partial void OnSplinterAmount1Changed(decimal value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    partial void OnSplinterAmount2Changed(decimal value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }
}
