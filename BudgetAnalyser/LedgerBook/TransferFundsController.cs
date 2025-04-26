using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TransferFundsController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
{
    private Guid dialogCorrelationId;

    public TransferFundsController(IMessenger messenger) : base(messenger)
    {
        if (messenger is null)
        {
            throw new ArgumentNullException(nameof(messenger));
        }

        Messenger.Register<TransferFundsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public event EventHandler TransferFundsRequested;

    public bool BankTransferConfirmed { get; set; }

    public IEnumerable<LedgerBucket> LedgerBuckets { get; private set; }

    public TransferFundsCommand TransferFundsDto { get; set; }

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteCancelButton => true;

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteOkButton => false;

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteSaveButton => IsOkToSave();

    public string ActionButtonToolTip => "Save and action the transfer.";

    public string CloseButtonToolTip => "Cancel the transfer.";

    public void RequerySuggested()
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    public void ShowDialog(IEnumerable<LedgerBucket> ledgerBuckets)
    {
        Reset();
        TransferFundsDto = new TransferFundsCommand();
        LedgerBuckets = ledgerBuckets.ToList();
        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.SaveCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Transfer Funds"
        };
        Messenger.Send(dialogRequest);
    }

    private bool IsOkToSave()
    {
        if (!TransferFundsDto.IsValid)
        {
            return false;
        }

        return TransferFundsDto.BankTransferRequired ? BankTransferConfirmed : true;
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (message.Response == ShellDialogButton.Cancel)
        {
            Reset();
            return;
        }

        TransferFundsRequested?.Invoke(this, EventArgs.Empty);
        Reset();
    }

    private void Reset()
    {
        TransferFundsDto = null;
        BankTransferConfirmed = false;
        LedgerBuckets = null;
    }
}
