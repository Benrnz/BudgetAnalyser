using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ChooseBudgetBucketController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly IBudgetBucketRepository bucketRepository;
        private Guid dialogCorrelationId;
        private IEnumerable<BudgetBucket> doNotUseBudgetBuckets;
        private string doNotUseFilterDescription;
        private bool filtered;
        private BudgetBucket? doNotUseSelected;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public ChooseBudgetBucketController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository, [NotNull] IAccountTypeRepository accountRepo)
            : base(uiContext.Messenger)
        {
            if (uiContext is null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
            this.accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
            BudgetBuckets = bucketRepository.Buckets.ToList();

            Messenger.Register<ChooseBudgetBucketController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        }

        // TODO Change this to a message:
        public event EventHandler<BudgetBucketChosenEventArgs> Chosen;
        public string ActionButtonToolTip => "Select and use this Expense Budget Bucket.";

        [UsedImplicitly]
        public IEnumerable<Account> BankAccounts => this.accountRepo.ListCurrentlyUsedAccountTypes();

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            [UsedImplicitly] get => this.doNotUseBudgetBuckets;

            private set
            {
                this.doNotUseBudgetBuckets = value;
                OnPropertyChanged();
            }
        }

        public bool CanExecuteCancelButton => true;
        public bool CanExecuteOkButton => Selected is not null;
        public bool CanExecuteSaveButton => false;
        public string CloseButtonToolTip => "Cancel";

        public string FilterDescription
        {
            [UsedImplicitly] get => this.doNotUseFilterDescription;
            set
            {
                if (value == this.doNotUseFilterDescription) return;
                this.doNotUseFilterDescription = value;
                OnPropertyChanged();
            }
        }

        public BudgetBucket? Selected
        {
            get => this.doNotUseSelected;
            set
            {
                if (Equals(value, this.doNotUseSelected)) return;
                this.doNotUseSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecuteOkButton));
                Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
            }
        }

        public bool ShowBankAccount { get; set; }

        public Account? StoreInThisAccount { get; set; }

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

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            EventHandler<BudgetBucketChosenEventArgs> handler = Chosen;
            if (handler is not null)
            {
                if (message.Response == ShellDialogButton.Cancel)
                {
                    handler(this, new BudgetBucketChosenEventArgs(this.dialogCorrelationId, true));
                }
                else
                {
                    handler(this, new BudgetBucketChosenEventArgs(this.dialogCorrelationId, Selected, StoreInThisAccount));
                }
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
}