using System;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CreateNewFixedBudgetController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private Guid dialogCorrelationId;
        private decimal doNotUseAmount;
        private string doNotUseCode;
        private string doNotUseDescription;
        private IUserMessageBox messageBox;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public CreateNewFixedBudgetController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public event EventHandler<DialogResponseEventArgs> Complete;

        public decimal Amount
        {
            get { return this.doNotUseAmount; }
            set
            {
                this.doNotUseAmount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Code)
                       && !this.bucketRepository.IsValidCode(FixedBudgetProjectBucket.CreateCode(Code))
                       && !string.IsNullOrWhiteSpace(Description)
                       && Amount > 0;
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public string Code
        {
            get { return this.doNotUseCode; }
            set
            {
                this.doNotUseCode = value;
                RaisePropertyChanged();
            }
        }

        public string Description
        {
            get { return this.doNotUseDescription; }
            set
            {
                this.doNotUseDescription = value;
                RaisePropertyChanged();
            }
        }

        public void ShowDialog(BudgetAnalyserFeature source, Guid? correlationId = null)
        {
            if (correlationId == null)
            {
                this.dialogCorrelationId = Guid.NewGuid();
            }
            else
            {
                this.dialogCorrelationId = correlationId.Value;
            }

            var dialogRequest = new ShellDialogRequestMessage(source, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Create new fixed budget project",
                HelpAvailable = true,
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Help)
            {
                this.messageBox.Show("Using this feature you can create a special temporary budget bucket with a fixed total budget amount.  There is no monthly budget contribution, just a fixed total. This allows tracking of a specific project against a budget, and will show a bar chart as the budget gets lower.  Transactions must be tagged with the bucket code using the bucket code in the Transactions view.  All tagged transactions are still considered as surplus (in fact this new bucket will inherit from the Surplus bucket).");
                return;
            }

            var handler = Complete;
            if (handler != null)
            {
                handler(this, new DialogResponseEventArgs(this.dialogCorrelationId, message.Response == ShellDialogButton.Cancel));
            }
        }
    }
}