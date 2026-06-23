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
            CalculateSplinter1Command = new DelegateCommand(CalculateSplinter1);
            CalculateSplinter2Command = new DelegateCommand(CalculateSplinter2);
    }

    public IEnumerable<BudgetBucket> BudgetBuckets { get; private set; } = Array.Empty<BudgetBucket>();

    public string? InvalidMessage
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

    public Transaction? OriginalTransaction
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
                        OnPropertyChanged();
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
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(Valid));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public BudgetBucket? SplinterBucket1 { get; set; }
    public BudgetBucket? SplinterBucket2 { get; set; }
    public decimal TotalAmount => SplinterAmount1 + SplinterAmount2;

    public System.Windows.Input.ICommand CalculateSplinter1Command { get; }
    public System.Windows.Input.ICommand CalculateSplinter2Command { get; }

    private void CalculateSplinter1()
    {
        if (OriginalTransaction == null) return;
        var other = decimal.Round(this.doNotUseSplinterAmount2, 2);
        var calculated = decimal.Round(OriginalTransaction.Amount - other, 2);
        this.doNotUseSplinterAmount1 = calculated;
        OnPropertyChanged(nameof(SplinterAmount1));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(Valid));
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    private void CalculateSplinter2()
    {
        if (OriginalTransaction == null) return;
        var other = decimal.Round(this.doNotUseSplinterAmount1, 2);
        var calculated = decimal.Round(OriginalTransaction.Amount - other, 2);
        this.doNotUseSplinterAmount2 = calculated;
        OnPropertyChanged(nameof(SplinterAmount2));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(Valid));
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    private class DelegateCommand : System.Windows.Input.ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => this.execute();
    }

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

            if (decimal.Round(SplinterAmount1 + SplinterAmount2, 2) != decimal.Round(OriginalTransaction.Amount, 2))
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
