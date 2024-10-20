using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class TransferFundsController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private Guid dialogCorrelationId;

        public TransferFundsController([NotNull] IMessenger messenger) : base(messenger)
        {
            if (messenger is null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            Messenger.Register<TransferFundsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        }

        public event EventHandler TransferFundsRequested;

        public string ActionButtonToolTip => "Save and action the transfer.";

        public bool BankTransferConfirmed { get; set; }

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

        public string CloseButtonToolTip => "Cancel the transfer.";

        public IEnumerable<LedgerBucket> LedgerBuckets { get; private set; }

        public TransferFundsCommand TransferFundsDto { get; set; }

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
            if (!TransferFundsDto.IsValid) return false;
            if (TransferFundsDto.BankTransferRequired)
                return BankTransferConfirmed;

            return true;
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
}