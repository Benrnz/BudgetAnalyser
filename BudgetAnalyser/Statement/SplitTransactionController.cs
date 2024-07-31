using System.Globalization;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class SplitTransactionController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private Guid dialogCorrelationId;
        private string doNotUseInvalidMessage;
        private decimal doNotUseSplinterAmount1;
        private decimal doNotUseSplinterAmount2;

        public SplitTransactionController([NotNull] UiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            this.bucketRepo = bucketRepo;
            Messenger = uiContext.Messenger;
            Messenger.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public string ActionButtonToolTip => "Save Split Transactions.";
        public IEnumerable<BudgetBucket> BudgetBuckets { [UsedImplicitly] get; private set; }
        public bool CanExecuteCancelButton => true;
        public bool CanExecuteOkButton => false;
        public bool CanExecuteSaveButton => Valid;
        public string CloseButtonToolTip => "Cancel.";

        public string InvalidMessage
        {
            [UsedImplicitly] get { return this.doNotUseInvalidMessage; }
            private set
            {
                this.doNotUseInvalidMessage = value;
                OnPropertyChanged();
            }
        }

        public Transaction OriginalTransaction { get; private set; }

        public decimal SplinterAmount1
        {
            get { return this.doNotUseSplinterAmount1; }
            set
            {
                this.doNotUseSplinterAmount1 = value;
                this.doNotUseSplinterAmount2 = OriginalTransaction.Amount - value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SplinterAmount2));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(Valid));
            }
        }

        public decimal SplinterAmount2
        {
            get { return this.doNotUseSplinterAmount2; }
            set
            {
                this.doNotUseSplinterAmount2 = value;
                this.doNotUseSplinterAmount1 = OriginalTransaction.Amount - value;
                OnPropertyChanged(nameof(SplinterAmount1));
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(Valid));
            }
        }

        public BudgetBucket SplinterBucket1 { get; set; }
        public BudgetBucket SplinterBucket2 { get; set; }
        public decimal TotalAmount => SplinterAmount1 + SplinterAmount2;

        public bool Valid
        {
            get
            {
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

                if (SplinterAmount1 + SplinterAmount2 != OriginalTransaction.Amount)
                {
                    InvalidMessage = string.Format(CultureInfo.CurrentCulture, "The two amounts do not add up to {0:C}", OriginalTransaction.Amount);
                    return false;
                }

                return true;
            }
        }

        public void ShowDialog(Transaction originalTransaction, Guid correlationId)
        {
            BudgetBuckets = this.bucketRepo.Buckets;
            this.dialogCorrelationId = correlationId;
            OriginalTransaction = originalTransaction;
            SplinterAmount1 = OriginalTransaction.Amount;
            SplinterAmount2 = 0M;
            SplinterBucket2 = SplinterBucket1 = OriginalTransaction.BudgetBucket;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = correlationId,
                Title = "Split Transaction"
            };
            Messenger.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            // StatementController processes the request to add the two new transactions.
            this.dialogCorrelationId = Guid.Empty;
            OriginalTransaction = null;
        }
    }
}