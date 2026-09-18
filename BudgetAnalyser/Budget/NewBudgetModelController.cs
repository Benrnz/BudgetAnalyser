using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Budget;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once ClassNeverInstantiated.Global
public partial class NewBudgetModelController : ControllerBase, IShellDialogInteractivity
{
    private readonly IUserMessageBox messageBox;
    private Guid dialogCorrelationId;

    public NewBudgetModelController(IMessenger messenger, UserPrompts userPrompts) : base(messenger)
    {
        Messenger.Register<NewBudgetModelController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        this.messageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        BudgetCycle = BudgetCycle.Monthly;
    }

    /// <summary>
    ///     Gets the pay cycle for this budget. Can only be set during budget creation.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FortnightlyChecked))]
    [NotifyPropertyChangedFor(nameof(MonthlyChecked))]
    public partial BudgetCycle BudgetCycle { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExecuteSaveButton))]
    public partial DateOnly EffectiveFrom { get; set; }

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
    public bool CanExecuteSaveButton => EffectiveFrom >= DateOnlyExt.Today();

    public void ShowDialog(DateOnly defaultEffectiveDate)
    {
        this.dialogCorrelationId = Guid.NewGuid();
        EffectiveFrom = defaultEffectiveDate;

        var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, this, ShellDialogType.SaveCancel)
        {
            CorrelationId = this.dialogCorrelationId, Title = "Create new Budget based on current", HelpAvailable = true
        };
        Messenger.Send(dialogRequest);
    }

    partial void OnEffectiveFromChanged(DateOnly value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
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
            Messenger.Send(new NewBudgetModelReadyMessage(this.dialogCorrelationId, EffectiveFrom, BudgetCycle));
        }
    }
}
