using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Budget;

[AutoRegisterWithIoC(SingleInstance = true)]
public partial class ChooseBudgetBucketController : ControllerBase, IShellDialogInteractivity
{
    private readonly IAccountTypeRepository accountRepo;
    private readonly IBudgetBucketRepository bucketRepository;
    private Guid dialogCorrelationId;
    private bool filtered;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
    public ChooseBudgetBucketController(IMessenger messenger, IBudgetBucketRepository bucketRepository, IAccountTypeRepository accountRepo)
        : base(messenger)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
        BudgetBuckets = bucketRepository.Buckets.ToList();

        Messenger.Register<ChooseBudgetBucketController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public IEnumerable<Account> BankAccounts => this.accountRepo.ListCurrentlyUsedAccountTypes();

    [ObservableProperty]
    public partial IEnumerable<BudgetBucket> BudgetBuckets { get; private set; }

    [ObservableProperty]
    public partial string FilterDescription { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExecuteOkButton))]
    public partial BudgetBucket? Selected { get; set; }

    public bool ShowBankAccount { get; set; }

    public Account? StoreInThisAccount { get; set; }

    public bool CanExecuteCancelButton => true;
    public bool CanExecuteOkButton => Selected is not null;
    public bool CanExecuteSaveButton => false;

    public void Filter(Func<BudgetBucket, bool> predicate, string filterDescription)
    {
        FilterDescription = filterDescription;
        BudgetBuckets = this.bucketRepository.Buckets.Where(predicate).ToList();
        this.filtered = true;
    }

    public void ShowDialog(BudgetAnalyserFeature source, string title, Guid? correlationId = null, bool showBankAccountSelector = false)
    {
        this.dialogCorrelationId = correlationId ?? Guid.NewGuid();

        ShowBankAccount = showBankAccountSelector;

        var dialogRequest = new ShellDialogRequestMessage(source, this, ShellDialogType.OkCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = title
        };
        Messenger.Send(dialogRequest);
    }

    partial void OnSelectedChanged(BudgetBucket? value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (message.Response == ShellDialogButton.Cancel)
        {
            Messenger.Send(new BudgetBucketChosenMessage(this, this.dialogCorrelationId, true));
        }
        else
        {
            Messenger.Send(new BudgetBucketChosenMessage(this, this.dialogCorrelationId, Selected, StoreInThisAccount));
        }

        Reset();
    }

    private void Reset()
    {
        if (this.filtered)
        {
            BudgetBuckets = this.bucketRepository.Buckets.ToList();
        }

        Selected = null;
        StoreInThisAccount = null;
    }
}
