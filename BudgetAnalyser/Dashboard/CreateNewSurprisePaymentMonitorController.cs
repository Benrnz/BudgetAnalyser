using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CreateNewSurprisePaymentMonitorController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IUserMessageBox messageBox;
        private Guid dialogCorrelationId;
        private WeeklyOrFortnightly doNotUseFrequency;
        private DateTime doNotUsePaymentStartDate;
        private BudgetBucket? doNotUseSelected;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public CreateNewSurprisePaymentMonitorController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository) : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
            Messenger.Register<CreateNewSurprisePaymentMonitorController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
            PaymentStartDate = DateTime.Today;
            Frequency = WeeklyOrFortnightly.Weekly;
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        // TODO Replace this event with a message.
        public event EventHandler<DialogResponseEventArgs> Complete;

        [UsedImplicitly]
        public IEnumerable<BudgetBucket> BudgetBuckets => this.bucketRepository.Buckets.ToList();

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton => true;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton => Selected! != null! && PaymentStartDate != DateTime.MinValue;

        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => false;

        public WeeklyOrFortnightly Frequency
        {
            get { return this.doNotUseFrequency; }
            set
            {
                this.doNotUseFrequency = value;
                OnPropertyChanged();
            }
        }

        public DateTime PaymentStartDate
        {
            get => this.doNotUsePaymentStartDate;
            set
            {
                if (Equals(value, this.doNotUsePaymentStartDate)) return;
                this.doNotUsePaymentStartDate = value;
                OnPropertyChanged();
                Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
            }
        }

        public BudgetBucket? Selected
        {
            get => this.doNotUseSelected;
            [UsedImplicitly]
            set
            {
                if (Equals(value, this.doNotUseSelected)) return;
                this.doNotUseSelected = value;
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
                Title = "Create new surprise regular payment monitor",
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
                    "Using this feature you can create a widget to show upcoming months where that require more than usual weekly or fortnightly payments. This is because there is an uneven number of weeks per month, so occasionally there will be 5 weekly payments in one month and 3 fortnightly payments. This widget will show the months where this will occur for the given Budget Bucket.");
                return;
            }

            EventHandler<DialogResponseEventArgs> handler = Complete;
            handler?.Invoke(this, new DialogResponseEventArgs(this.dialogCorrelationId, message.Response == ShellDialogButton.Cancel));
        }
    }
}