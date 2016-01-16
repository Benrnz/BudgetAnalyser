using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerRemarksController : ControllerBase
    {
        private readonly IReconciliationService reconciliationService;
        private Guid dialogCorrelationId;
        private bool doNotUseIsReadOnly;
        private LedgerEntryLine doNotUseLedgerEntryLine;
        private string doNotUseRemarks;

        public LedgerRemarksController([NotNull] UiContext uiContext, [NotNull] IReconciliationService reconciliationService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (reconciliationService == null)
            {
                throw new ArgumentNullException(nameof(reconciliationService));
            }

            this.reconciliationService = reconciliationService;
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler Completed;

        public bool IsReadOnly
        {
            get { return this.doNotUseIsReadOnly; }
            private set
            {
                this.doNotUseIsReadOnly = value;
                RaisePropertyChanged();
            }
        }

        public LedgerEntryLine LedgerEntryLine
        {
            get { return this.doNotUseLedgerEntryLine; }
            private set
            {
                this.doNotUseLedgerEntryLine = value;
                RaisePropertyChanged();
            }
        }

        public string Remarks
        {
            get { return this.doNotUseRemarks; }
            set
            {
                this.doNotUseRemarks = value;
                RaisePropertyChanged();
            }
        }

        public void Show(LedgerEntryLine line, bool isNew)
        {
            LedgerEntryLine = line;
            Remarks = LedgerEntryLine.Remarks;
            IsReadOnly = !isNew;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.Ok)
            {
                Title = "Ledger Entry Remarks",
                CorrelationId = this.dialogCorrelationId
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (!IsReadOnly)
            {
                this.reconciliationService.UpdateRemarks(LedgerEntryLine, Remarks);
            }

            LedgerEntryLine = null;
            Remarks = null;

            EventHandler handler = Completed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}