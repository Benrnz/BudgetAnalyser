using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC(SingleInstance = true)]
public class LedgerRemarksController : ControllerBase
{
    private readonly IReconciliationService reconciliationService;
    private Guid dialogCorrelationId;

    public LedgerRemarksController(IMessenger messenger, IReconciliationService reconciliationService) : base(messenger)
    {
        this.reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
        Messenger.Register<LedgerRemarksController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public bool IsReadOnly
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public LedgerEntryLine? LedgerEntryLine
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string Remarks
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

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
        Messenger.Send(dialogRequest);
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (!IsReadOnly && LedgerEntryLine is not null)
        {
            this.reconciliationService.UpdateRemarks(LedgerEntryLine, Remarks);
        }

        LedgerEntryLine = null;
        Remarks = string.Empty;
        Messenger.Send<LedgerRemarksCompletedMessage>();
    }
}
