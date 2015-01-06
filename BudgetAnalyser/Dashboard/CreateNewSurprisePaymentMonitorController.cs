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
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CreateNewSurprisePaymentMonitorController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private Guid dialogCorrelationId;
        private DateTime doNotUsePaymentStartDate;
        private BudgetBucket doNotUseSelected;
        private WeeklyOrFortnightly doNotUseFrequency;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public CreateNewSurprisePaymentMonitorController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository)
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
            PaymentStartDate = DateTime.Today;
            Frequency = WeeklyOrFortnightly.Weekly;
        }

        public event EventHandler<DialogResponseEventArgs> Complete;

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            get { return this.bucketRepository.Buckets.ToList(); }
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

        public WeeklyOrFortnightly Frequency
        {
            get { return this.doNotUseFrequency; }
            set
            {
                this.doNotUseFrequency = value;
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
                return Selected != null && PaymentStartDate != DateTime.MinValue;
            }
        }

        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton
        {
            get { return false; }
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
                Title = "Create new surprise regular payment monitor"
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
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