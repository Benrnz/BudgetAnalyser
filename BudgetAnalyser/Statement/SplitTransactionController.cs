using System.Globalization;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class SplitTransactionController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
{
    private readonly IBudgetBucketRepository bucketRepo;
    private Guid dialogCorrelationId;
    private string? doNotUseInvalidMessage;
    private Transaction? doNotUseOriginalTransaction;
    private decimal doNotUseSplinterAmount1;
    private decimal doNotUseSplinterAmount2;

    public SplitTransactionController(IUiContext uiContext, IBudgetBucketRepository bucketRepo) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        Messenger.Register<SplitTransactionController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public IEnumerable<BudgetBucket> BudgetBuckets { get; private set; } = Array.Empty<BudgetBucket>();

    public string? InvalidMessage
    {
        get => this.doNotUseInvalidMessage;
        private set
        {
            if (value == this.doNotUseInvalidMessage)
            {
                return;
            }

            this.doNotUseInvalidMessage = value;
            OnPropertyChanged();
        }
    }

    public Transaction? OriginalTransaction
    {
        get => this.doNotUseOriginalTransaction;
        private set
        {
            if (Equals(value, this.doNotUseOriginalTransaction))
            {
                return;
            }

            this.doNotUseOriginalTransaction = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Valid));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public decimal SplinterAmount1
    {
        get => this.doNotUseSplinterAmount1;
        set
        {
            if (value == this.doNotUseSplinterAmount1)
            {
                return;
            }

            this.doNotUseSplinterAmount1 = value;
            this.doNotUseSplinterAmount2 = OriginalTransaction?.Amount ?? 0 - value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SplinterAmount2));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(Valid));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public decimal SplinterAmount2
    {
        get => this.doNotUseSplinterAmount2;
        set
        {
            if (value == this.doNotUseSplinterAmount2)
            {
                return;
            }

            this.doNotUseSplinterAmount2 = value;
            this.doNotUseSplinterAmount1 = OriginalTransaction?.Amount ?? 0 - value;
            OnPropertyChanged(nameof(SplinterAmount1));
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(Valid));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

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

            if (SplinterAmount1 + SplinterAmount2 != OriginalTransaction.Amount)
            {
                InvalidMessage = string.Format(CultureInfo.CurrentCulture, "The two amounts do not add up to {0:C}", OriginalTransaction.Amount);
                return false;
            }

            return true;
        }
    }

    public bool CanExecuteCancelButton => true;
    public bool CanExecuteOkButton => false;
    public bool CanExecuteSaveButton => Valid;

    public string ActionButtonToolTip => "Save Split Transactions.";
    public string CloseButtonToolTip => "Cancel.";

    public void ShowDialog(Transaction originalTransaction, Guid correlationId)
    {
        BudgetBuckets = this.bucketRepo.Buckets;
        this.dialogCorrelationId = correlationId;
        OriginalTransaction = originalTransaction;
        SplinterAmount1 = OriginalTransaction.Amount;
        SplinterAmount2 = 0M;
        SplinterBucket2 = SplinterBucket1 = originalTransaction.BudgetBucket;

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
