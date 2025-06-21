using System.ComponentModel;
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
    private LedgerEntryLine? ledgerEntryLine;

    public TransferFundsController(IMessenger messenger) : base(messenger)
    {
        if (messenger is null)
        {
            throw new ArgumentNullException(nameof(messenger));
        }

        Messenger.Register<TransferFundsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public event EventHandler? TransferFundsRequested;

    public bool BankTransferConfirmed { get; set; }

    public decimal? FromBalance
    {
        get
        {
            if (TransferFundsDto.FromLedger is SurplusLedger)
            {
                return this.ledgerEntryLine?.SurplusBalances.Single(x => x.Account == TransferFundsDto.FromLedger.StoredInAccount).Balance;
            }

            return this.ledgerEntryLine?.Entries.Single(x => TransferFundsDto.FromLedger == x.LedgerBucket).Balance;
        }
    }

    public IEnumerable<LedgerBucket> LedgerBuckets { get; private set; } = Array.Empty<LedgerBucket>();

    public decimal? ToBalance
    {
        get
        {
            if (TransferFundsDto.ToLedger is SurplusLedger)
            {
                return this.ledgerEntryLine?.SurplusBalances.Single(x => x.Account == TransferFundsDto.ToLedger.StoredInAccount).Balance;
            }

            return this.ledgerEntryLine?.Entries.Single(x => TransferFundsDto.ToLedger == x.LedgerBucket).Balance;
        }
    }

    public TransferFundsCommand TransferFundsDto { get; private set; } = new();

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

    public void ShowDialog(IEnumerable<LedgerBucket> ledgerBuckets, LedgerEntryLine currentLedgerEntryLine)
    {
        Reset();
        this.ledgerEntryLine = currentLedgerEntryLine ?? throw new ArgumentNullException(nameof(currentLedgerEntryLine));
        LedgerBuckets = ledgerBuckets.ToList();
        this.dialogCorrelationId = Guid.NewGuid();
        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.SaveCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Transfer Funds"
        };
        Messenger.Send(dialogRequest);
        TransferFundsDto.PropertyChanged += OnTransferFundsDtoPropertyChanged;
    }

    private bool IsOkToSave()
    {
        if (!TransferFundsDto.IsValid)
        {
            return false;
        }

        return !TransferFundsDto.BankTransferRequired || BankTransferConfirmed;
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

    private void OnTransferFundsDtoPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TransferFundsDto.FromLedger):
                OnPropertyChanged(nameof(FromBalance));
                break;
            case nameof(TransferFundsDto.ToLedger):
                OnPropertyChanged(nameof(ToBalance));
                break;
        }
    }

    private void Reset()
    {
        TransferFundsDto.PropertyChanged -= OnTransferFundsDtoPropertyChanged;
        this.ledgerEntryLine = null;
        TransferFundsDto = new TransferFundsCommand();
        BankTransferConfirmed = false;
        LedgerBuckets = Array.Empty<LedgerBucket>();
    }
}
