using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Budget;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once ClassNeverInstantiated.Global
public class NewBudgetModelController : ControllerBase, IShellDialogInteractivity
{
    private readonly IUserMessageBox messageBox;
    private Guid dialogCorrelationId;
    private BudgetCycle doNotUseBudgetCycle;
    private DateTime doNotUseEffectiveFrom;

    public NewBudgetModelController([NotNull] IUiContext uiContext) : base(uiContext.Messenger)
    {
        if (uiContext == null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        Messenger.Register<NewBudgetModelController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        this.messageBox = uiContext.UserPrompts.MessageBox;
        BudgetCycle = BudgetCycle.Monthly;
    }

    public event EventHandler Ready;

    /// <summary>
    ///     Gets the pay cycle for this budget. Can only be set during budget creation.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public BudgetCycle BudgetCycle
    {
        get => this.doNotUseBudgetCycle;
        set
        {
            if (value == this.doNotUseBudgetCycle) return;
            this.doNotUseBudgetCycle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FortnightlyChecked));
            OnPropertyChanged(nameof(MonthlyChecked));
        }
    }

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
    public bool CanExecuteSaveButton => EffectiveFrom > DateTime.Today;

    // ReSharper disable once MemberCanBePrivate.Global
    public DateTime EffectiveFrom
    {
        get => this.doNotUseEffectiveFrom;
        set
        {
            if (value == this.doNotUseEffectiveFrom) return;
            this.doNotUseEffectiveFrom = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    // ReSharper disable once UnusedMember.Global
    public bool FortnightlyChecked
    {
        get => BudgetCycle == BudgetCycle.Fortnightly;
        set => BudgetCycle = value ? BudgetCycle.Fortnightly : BudgetCycle.Monthly;
    }

    // ReSharper disable once UnusedMember.Global
    public bool MonthlyChecked
    {
        get => BudgetCycle == BudgetCycle.Monthly;
        set => BudgetCycle = value ? BudgetCycle.Monthly : BudgetCycle.Fortnightly;
    }

    public void ShowDialog(DateTime defaultEffectiveDate)
    {
        this.dialogCorrelationId = Guid.NewGuid();
        EffectiveFrom = defaultEffectiveDate;

        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, this, ShellDialogType.SaveCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Create new Budget based on current",
            HelpAvailable = true
        };
        Messenger.Send(dialogRequest);
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (message.Response == ShellDialogButton.Help)
        {
            this.messageBox.Show("This will clone an existing budget, the currently shown budget, to a new budget that is future dated.  The budget must have an effective date in the future.");
            return;
        }

        if (message.Response != ShellDialogButton.Cancel)
        {
            Ready?.Invoke(this, EventArgs.Empty);
        }
    }
}