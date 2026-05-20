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
    private bool doNotUseIsReadOnly;
    private LedgerEntryLine? doNotUseLedgerEntryLine;
    private string doNotUseRemarks = string.Empty;

    public LedgerRemarksController(IUiContext uiContext, IReconciliationService reconciliationService) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
        Messenger.Register<LedgerRemarksController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public event EventHandler? Completed;

    public bool IsReadOnly
    {
        get => this.doNotUseIsReadOnly;
        private set
        {
            this.doNotUseIsReadOnly = value;
            OnPropertyChanged();
        }
    }

    public LedgerEntryLine? LedgerEntryLine
    {
        get => this.doNotUseLedgerEntryLine;
        private set
        {
            this.doNotUseLedgerEntryLine = value;
            OnPropertyChanged();
        }
    }

    public string Remarks
    {
        get => this.doNotUseRemarks;
        set
        {
            this.doNotUseRemarks = value;
            OnPropertyChanged();
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

        var handler = Completed;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
