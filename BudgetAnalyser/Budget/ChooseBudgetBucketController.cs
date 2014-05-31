using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ChooseBudgetBucketController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private Guid dialogCorrelationId;
        private IEnumerable<BudgetBucket> doNotUseBudgetBuckets;
        private string doNotUseFilterDescription;
        private bool filtered;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public ChooseBudgetBucketController([NotNull] UiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository)
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
            BudgetBuckets = bucketRepository.Buckets.ToList();

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler Chosen;

        public string ActionButtonToolTip
        {
            get { return "Select and use this Expense Budget Bucket."; }
        }

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            get { return this.doNotUseBudgetBuckets; }

            private set
            {
                this.doNotUseBudgetBuckets = value;
                RaisePropertyChanged(() => BudgetBuckets);
            }
        }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return Selected != null; }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public string FilterDescription
        {
            get { return this.doNotUseFilterDescription; }
            set
            {
                this.doNotUseFilterDescription = value;
                RaisePropertyChanged(() => FilterDescription);
            }
        }

        public BudgetBucket Selected { get; set; }

        public void Filter(Func<BudgetBucket, bool> predicate, string filterDescription)
        {
            FilterDescription = filterDescription;
            BudgetBuckets = this.bucketRepository.Buckets.Where(predicate).ToList();
            this.filtered = true;
        }

        public void ShowDialog(BudgetAnalyserFeature source)
        {
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(source, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Add New Ledger to Ledger Book",
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void CompleteSelection()
        {
            EventHandler handler = Chosen;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            FilterDescription = null;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            ResetFilter();
            if (message.Response == ShellDialogButton.Cancel)
            {
                Selected = null;
            }

            CompleteSelection();
        }

        private void ResetFilter()
        {
            if (this.filtered)
            {
                BudgetBuckets = this.bucketRepository.Buckets.ToList();
            }
        }
    }
}