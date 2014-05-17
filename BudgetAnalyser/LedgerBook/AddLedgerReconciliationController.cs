using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class AddLedgerReconciliationController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private Guid dialogCorrelationId;
        private decimal doNotUseBankBalance;
        private DateTime doNotUseDate;

        public AddLedgerReconciliationController([NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler Complete;

        public string ActionButtonToolTip
        {
            get { return "Add new ledger entry line."; }
        }

        public decimal BankBalance
        {
            get { return this.doNotUseBankBalance; }
            set
            {
                this.doNotUseBankBalance = value;
                RaisePropertyChanged(() => BankBalance);
            }
        }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return Date != DateTime.MinValue; }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public bool Canceled { get; private set; }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                RaisePropertyChanged(() => Date);
            }
        }

        public void ShowDialog()
        {
            Date = DateTime.Today;
            Canceled = false;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "New Reconciliation",
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (this.dialogCorrelationId != message.CorrelationId)
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                Canceled = true;
            }

            EventHandler handler = Complete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}