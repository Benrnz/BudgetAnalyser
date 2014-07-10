using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement
{
    public class StatementModel : INotifyPropertyChanged
    {
        private readonly ILogger logger;
        private GlobalFilterCriteria currentFilter;
        private List<Transaction> doNotUseAllTransactions;
        private int doNotUseDurationInMonths;
        private IEnumerable<Transaction> doNotUseTransactions;
        private IEnumerable<IGrouping<int, Transaction>> duplicates;

        private int fullDuration;

        public StatementModel([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
            ChangeHash = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<Transaction> AllTransactions
        {
            get { return this.doNotUseAllTransactions; }

            private set { this.doNotUseAllTransactions = value.ToList(); }
        }

        /// <summary>
        ///     A hash to show when critical state of the statement model has changed. Includes child objects ie Transactions.
        ///     The hash does not persist between Application Loads.
        /// </summary>
        public Guid ChangeHash { get; private set; }

        public int DurationInMonths
        {
            get { return this.doNotUseDurationInMonths; }

            set
            {
                this.doNotUseDurationInMonths = value;
                OnPropertyChanged();
            }
        }

        public string FileName { get; set; }
        public bool Filtered { get; private set; }
        public DateTime LastImport { get; set; }

        public IEnumerable<Transaction> Transactions
        {
            get { return this.doNotUseTransactions; }

            private set
            {
                this.doNotUseTransactions = value;
                ChangeHash = Guid.NewGuid();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Calculates the duration in months from the beginning of the period to the end.
        /// </summary>
        /// <param name="criteria">
        ///     The criteria that is currently applied to the Statement. Pass in null to use first and last
        ///     statement dates.
        /// </param>
        /// <param name="transactions">The list of transactions to use to determine duration.</param>
        public static int CalculateDuration(GlobalFilterCriteria criteria, IEnumerable<Transaction> transactions)
        {
            List<Transaction> list = transactions.ToList();
            DateTime minDate = DateTime.MaxValue, maxDate = DateTime.MinValue;

            if (criteria != null && !criteria.Cleared)
            {
                if (criteria.BeginDate != null)
                {
                    minDate = criteria.BeginDate.Value;
                    Debug.Assert(criteria.EndDate != null);
                    maxDate = criteria.EndDate.Value;
                }
            }
            else
            {
                minDate = list.Min(t => t.Date);
                maxDate = list.Max(t => t.Date);
            }

            return minDate.DurationInMonths(maxDate);
        }

        public void Filter(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                ChangeHash = Guid.NewGuid();
                Transactions = AllTransactions.ToList();
                DurationInMonths = this.fullDuration;
                Filtered = false;
                return;
            }

            if (criteria.BeginDate > criteria.EndDate)
            {
                throw new ArgumentException("End date must be after the begin date.");
            }

            this.currentFilter = criteria;

            ChangeHash = Guid.NewGuid();
            if (criteria.Cleared)
            {
                Transactions = AllTransactions.ToList();
                DurationInMonths = this.fullDuration;
                Filtered = false;
                return;
            }

            IEnumerable<Transaction> query = AllTransactions;
            if (criteria.BeginDate != null)
            {
                query = AllTransactions.Where(t => t.Date >= criteria.BeginDate.Value);
            }

            if (criteria.EndDate != null)
            {
                query = query.Where(t => t.Date <= criteria.EndDate.Value);
            }

            if (criteria.AccountType != null)
            {
                query = query.Where(t => t.AccountType == criteria.AccountType);
            }

            Transactions = query.ToList();
            DurationInMonths = CalculateDuration(criteria, Transactions);
            this.duplicates = null;
            Filtered = true;
        }

        public bool FilterByText(string textFilter)
        {
            if (string.IsNullOrWhiteSpace(textFilter))
            {
                return false;
            }

            if (textFilter.Length < 3)
            {
                return false;
            }

            // Do not modify the ChangeHash, this filter is not global, its only localised for a quick search of the data. It should not affect reports etc.
            Filtered = true;
            Transactions = Transactions.Where(t => MatchTransactionText(textFilter, t))
                .AsParallel()
                .ToList();

            return true;
        }

        public void Merge([NotNull] StatementModel additionalModel)
        {
            if (additionalModel == null)
            {
                throw new ArgumentNullException("additionalModel");
            }

            this.LastImport = additionalModel.LastImport;

            Merge(additionalModel.AllTransactions);
        }

        public void RemoveTransaction([NotNull] Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            transaction.PropertyChanged -= OnTransactionPropertyChanged;
            ChangeHash = Guid.NewGuid();
            this.doNotUseAllTransactions.Remove(transaction);
            Filter(this.currentFilter);
        }

        public void SplitTransaction(
            [NotNull] Transaction originalTransaction,
            decimal splinterAmount1,
            decimal splinterAmount2,
            [NotNull] BudgetBucket splinterBucket1,
            [NotNull] BudgetBucket splinterBucket2)
        {
            if (originalTransaction == null)
            {
                throw new ArgumentNullException("originalTransaction");
            }

            if (splinterBucket1 == null)
            {
                throw new ArgumentNullException("splinterBucket1");
            }

            if (splinterBucket2 == null)
            {
                throw new ArgumentNullException("splinterBucket2");
            }

            var splinterTransaction1 = (Transaction)originalTransaction.Clone();
            var splinterTransaction2 = (Transaction)originalTransaction.Clone();

            splinterTransaction1.Amount = splinterAmount1;
            splinterTransaction2.Amount = splinterAmount2;

            splinterTransaction1.BudgetBucket = splinterBucket1;
            splinterTransaction2.BudgetBucket = splinterBucket2;

            if (splinterAmount1 + splinterAmount2 != originalTransaction.Amount)
            {
                throw new InvalidOperationException("The two new amounts do not add up to the original transaction value.");
            }

            RemoveTransaction(originalTransaction);

            Merge(new[] { splinterTransaction1, splinterTransaction2 });
        }

        public IEnumerable<IGrouping<int, Transaction>> ValidateAgainstDuplicates()
        {
            if (this.duplicates != null)
            {
                return this.duplicates;
            }

            List<IGrouping<int, Transaction>> query = Transactions.GroupBy(t => t.GetEqualityHashCode(), t => t).Where(group => group.Count() > 1).ToList();
            this.logger.LogWarning(() => this.logger.Format("{0} Duplicates detected.", query.Sum(group => group.Count())));
            Parallel.ForEach(query, duplicate =>
            {
                foreach (Transaction txn in duplicate)
                {
                    txn.IsSuspectedDuplicate = true;
                }
            });
            this.duplicates = query;
            return this.duplicates;
        }

        /// <summary>
        ///     Used internally by the importers to load transactions into the statement model.
        /// </summary>
        /// <param name="transactions">The transactions to load.</param>
        /// <returns>Returns this instance, to allow chaining.</returns>
        internal virtual StatementModel LoadTransactions(IEnumerable<Transaction> transactions)
        {
            UnsubscribeToTransactionChangedEvents();
            ChangeHash = Guid.NewGuid();
            var listOfTransactions = transactions.OrderBy(t => t.Date).ToList();
            Transactions = listOfTransactions;
            AllTransactions = Transactions;
            if (listOfTransactions.Any())
            {
                this.fullDuration = CalculateDuration(new GlobalFilterCriteria(), AllTransactions);
                DurationInMonths = CalculateDuration(null, Transactions);
            }

            this.duplicates = null;
            OnPropertyChanged("Transactions");
            SubscribeToTransactionChangedEvents();
            return this;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static bool MatchTransactionText(string textFilter, Transaction t)
        {
            if (!string.IsNullOrWhiteSpace(t.Description))
            {
                if (t.Description.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference1))
            {
                if (t.Reference1.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference2))
            {
                if (t.Reference2.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference3))
            {
                if (t.Reference3.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void Merge([NotNull] IEnumerable<Transaction> additionalTransactions)
        {
            UnsubscribeToTransactionChangedEvents();
            ChangeHash = Guid.NewGuid();
            List<Transaction> mergedTransactions = AllTransactions.ToList().Merge(additionalTransactions).ToList();
            AllTransactions = mergedTransactions;
            this.duplicates = null;
            this.fullDuration = CalculateDuration(new GlobalFilterCriteria(), mergedTransactions);
            DurationInMonths = this.fullDuration;
            Filter(this.currentFilter);
            SubscribeToTransactionChangedEvents();
        }

        private void OnTransactionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case Transaction.AmountPropertyName:
                case Transaction.BucketPropertyName:
                case Transaction.DatePropertyName:
                    ChangeHash = Guid.NewGuid();
                    break;
            }
        }

        private void SubscribeToTransactionChangedEvents()
        {
            if (AllTransactions == null)
            {
                return;
            }

            Parallel.ForEach(AllTransactions, transaction => { transaction.PropertyChanged += OnTransactionPropertyChanged; });
        }

        private void UnsubscribeToTransactionChangedEvents()
        {
            if (AllTransactions == null)
            {
                return;
            }

            Parallel.ForEach(AllTransactions, transaction => { transaction.PropertyChanged -= OnTransactionPropertyChanged; });
        }
    }
}