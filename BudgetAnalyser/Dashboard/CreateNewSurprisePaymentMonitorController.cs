using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
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
        private BudgetBucket doNotUseSelected;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public CreateNewSurprisePaymentMonitorController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            this.bucketRepository = bucketRepository;
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            PaymentStartDate = DateTime.Today;
            Frequency = WeeklyOrFortnightly.Weekly;
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public event EventHandler<DialogResponseEventArgs> Complete;

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            get { return this.bucketRepository.Buckets.ToList(); }
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
            get { return Selected != null && PaymentStartDate != DateTime.MinValue; }
        }

        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public WeeklyOrFortnightly Frequency
        {
            get { return this.doNotUseFrequency; }
            set
            {
                this.doNotUseFrequency = value;
                RaisePropertyChanged();
            }
        }

        public DateTime PaymentStartDate
        {
            get { return this.doNotUsePaymentStartDate; }
            set
            {
                this.doNotUsePaymentStartDate = value;
                RaisePropertyChanged();
            }
        }

        public BudgetBucket Selected
        {
            get { return this.doNotUseSelected; }
            set
            {
                this.doNotUseSelected = value;
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
                Title = "Create new surprise regular payment monitor",
                HelpAvailable = true
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
                this.messageBox.Show(
                    "Using this feature you can create a widget to show upcoming months where that require more than usual weekly or fortnightly payments. This is because there is an uneven number of weeks per month, so occasionally there will be 5 weekly payments in one month and 3 fortnightly payments. This widget will show the months where this will occur for the given Budget Bucket.");
                return;
            }

            EventHandler<DialogResponseEventArgs> handler = Complete;
            if (handler != null)
            {
                handler(this, new DialogResponseEventArgs(this.dialogCorrelationId, message.Response == ShellDialogButton.Cancel));
            }
        }
    }
}