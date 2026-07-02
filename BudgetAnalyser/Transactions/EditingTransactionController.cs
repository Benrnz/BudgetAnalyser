using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public class EditingTransactionController(IMessenger messenger, IBudgetBucketRepository bucketRepo) : ControllerBase(messenger)
{
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private BudgetBucket? originalBucket;

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
}
