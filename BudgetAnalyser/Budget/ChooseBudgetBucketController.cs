using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ChooseBudgetBucketController : ControllerBase, IShowableController
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private IEnumerable<BudgetBucket> doNotUseBudgetBuckets;
        private string doNotUseFilterDescription;
        private bool doNotUseShown;
        private bool filtered;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public ChooseBudgetBucketController([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
            BudgetBuckets = bucketRepository.Buckets.ToList();
        }

        public event EventHandler Chosen;

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            get { return this.doNotUseBudgetBuckets; }

            private set
            {
                this.doNotUseBudgetBuckets = value;
                RaisePropertyChanged(() => BudgetBuckets);
            }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand(OnCancelCommandExecuted); }
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(OnCloseCommandExecuted); }
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

        public bool Shown
        {
            get { return this.doNotUseShown; }

            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public void Filter(Func<BudgetBucket, bool> predicate, string filterDescription)
        {
            FilterDescription = filterDescription;
            BudgetBuckets = this.bucketRepository.Buckets.Where(predicate).ToList();
            this.filtered = true;
        }

        private void CompleteSelection()
        {
            EventHandler handler = Chosen;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            Shown = false;
            FilterDescription = null;
        }

        private void OnCancelCommandExecuted()
        {
            ResetFilter();

            Selected = null;
            CompleteSelection();
        }

        private void OnCloseCommandExecuted()
        {
            ResetFilter();

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