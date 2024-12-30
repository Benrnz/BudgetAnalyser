using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CreateNewFixedBudgetController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IUserMessageBox messageBox;
        private Guid dialogCorrelationId;
        private decimal doNotUseAmount;
        private string doNotUseCode;
        private string doNotUseDescription;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public CreateNewFixedBudgetController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository) : base(uiContext.Messenger)
        {
            if (uiContext is null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
            Messenger.Register<CreateNewFixedBudgetController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public event EventHandler<DialogResponseEventArgs> Complete;

        public decimal Amount
        {
            get => this.doNotUseAmount;
            [UsedImplicitly]
            set
            {
                if (value == this.doNotUseAmount) return;
                this.doNotUseAmount = value;
                OnPropertyChanged();
                Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton => true;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton => !string.IsNullOrWhiteSpace(Code)
                                          && !this.bucketRepository.IsValidCode(FixedBudgetProjectBucket.CreateCode(Code))
                                          && !string.IsNullOrWhiteSpace(Description)
                                          && Amount > 0;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => false;

        public string Code
        {
            get => this.doNotUseCode;
            [UsedImplicitly]
            set
            {
                if (value == this.doNotUseCode) return;
                this.doNotUseCode = value;
                OnPropertyChanged();
                Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
            }
        }

        public string Description
        {
            get => this.doNotUseDescription;
            [UsedImplicitly]
            set
            {
                if (value == this.doNotUseDescription) return;
                this.doNotUseDescription = value;
                OnPropertyChanged();
                Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
            }
        }

        public void ShowDialog(BudgetAnalyserFeature source, Guid? correlationId = null)
        {
            this.dialogCorrelationId = correlationId ?? Guid.NewGuid();

            var dialogRequest = new ShellDialogRequestMessage(source, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Create new fixed budget project",
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
                this.messageBox.Show(
                    "Using this feature you can create a special temporary budget bucket with a fixed total budget amount.  There is no budget contribution, just a fixed total. This allows tracking of a specific project against a budget, and will show a bar chart as the budget gets lower.  Transactions must be tagged with the bucket code using the bucket code in the Transactions view.  All tagged transactions are still considered as surplus (in fact this new bucket will inherit from the Surplus bucket).");
                return;
            }

            EventHandler<DialogResponseEventArgs> handler = Complete;
            handler?.Invoke(this, new DialogResponseEventArgs(this.dialogCorrelationId, message.Response == ShellDialogButton.Cancel));
        }
    }
}