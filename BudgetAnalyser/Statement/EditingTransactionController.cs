using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class EditingTransactionController : ControllerBase
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private IEnumerable<BudgetBucket> doNotUseBuckets;
        private Transaction doNotUseTransaction;
        private BudgetBucket originalBucket;

        public EditingTransactionController([NotNull] UiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepo) : base(uiContext.Messenger)
        {
            if (uiContext is null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (bucketRepo is null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            this.bucketRepo = bucketRepo;
        }

        public IEnumerable<BudgetBucket> Buckets
        {
            [UsedImplicitly]
            get => this.doNotUseBuckets;
            private set
            {
                this.doNotUseBuckets = value;
                OnPropertyChanged();
            }
        }

        public bool HasChanged => OriginalHash != Transaction.GetEqualityHashCode()
                                  || this.originalBucket != Transaction.BudgetBucket;

        public int OriginalHash { get; private set; }

        public Transaction Transaction
        {
            get => this.doNotUseTransaction;
            set
            {
                OriginalHash = value?.GetEqualityHashCode() ?? 0;

                this.doNotUseTransaction = value;
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
}
