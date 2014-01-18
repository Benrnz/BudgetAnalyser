// TODO Move to Statement
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public class StatementModel : INotifyPropertyChanged
    {
        private int doNotUseDurationInMonths;
        private List<Transaction> doNotUseTransactions;
        private IEnumerable<IGrouping<int, Transaction>> duplicates;

        private int fullDuration;
        private GlobalFilterCriteria currentFilter;
        private List<Transaction> doNotUseAllTransactions;

        public event PropertyChangedEventHandler PropertyChanged;
        public IEnumerable<AccountType> AccountTypes { get; private set; }

        public IEnumerable<Transaction> AllTransactions
        {
            get
            {
                return this.doNotUseAllTransactions;
            }

            private set
            {
                this.doNotUseAllTransactions = value.ToList();
            }
        }

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
        public DateTime Imported { get; set; }

        public IEnumerable<Transaction> Transactions
        {
            get { return this.doNotUseTransactions; }

            private set
            {
                this.doNotUseTransactions = value.ToList();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Calculates the duration in months from the beginning of the period to the end.
        /// </summary>
        /// <param name="criteria">The criteria that is currently applied to the Statement. Pass in null to use first and last statement dates.</param>
        /// <param name="transactions">The list of transactions to use to determine duration.</param>
        public static int CalculateDuration(GlobalFilterCriteria criteria, IEnumerable<Transaction> transactions)
        {
            var list = transactions.ToList();
            DateTime minDate = DateTime.MaxValue, maxDate = DateTime.MinValue;
            bool needMinDate = true, needMaxDate = true;

            if (criteria != null && !criteria.Cleared && criteria.BeginDate != null)
            {
                minDate = criteria.BeginDate.Value;
                needMinDate = false;
            }

            if (criteria != null && !criteria.Cleared && criteria.EndDate != null)
            {
                maxDate = criteria.EndDate.Value;
                needMaxDate = false;
            }

            if (needMaxDate || needMinDate)
            {
                foreach (var transaction in list)
                {
                    if (needMinDate && transaction.Date < minDate)
                    {
                        minDate = transaction.Date;
                    }
                    
                    if (needMaxDate && transaction.Date > maxDate)
                    {
                        maxDate = transaction.Date;
                    }
                }
            }

            var durationInMonths = (int)Math.Round(maxDate.Subtract(minDate).TotalDays / 30, 0);
            if (durationInMonths <= 0)
            {
                durationInMonths = 1;
            }

            return durationInMonths;
        }

        public void Filter(GlobalFilterCriteria criteria)
        {
            if (criteria.BeginDate >= criteria.EndDate)
            {
                throw new ArgumentException("End date must be after the begin date.");
            }

            this.currentFilter = criteria;

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

        public StatementModel Merge(StatementModel additionalModel)
        {
            Imported = additionalModel.Imported;
            var mergedTransactions = AllTransactions.ToList().Merge(additionalModel.Transactions).ToList();
            AllTransactions = mergedTransactions;
            this.duplicates = null;
            this.fullDuration = CalculateDuration(new GlobalFilterCriteria(), mergedTransactions);
            DurationInMonths = this.fullDuration;
            AccountTypes = mergedTransactions.Select(t => t.AccountType).Distinct().ToList();
            Filter(this.currentFilter);
            return this;
        }

        public IEnumerable<IGrouping<int, Transaction>> ValidateAgainstDuplicates()
        {
            if (this.duplicates != null)
            {
                return this.duplicates;
            }

            List<IGrouping<int, Transaction>> query = Transactions.GroupBy(t => t.GetHashCode(), t => t).Where(group => group.Count() > 1).ToList();
            Debug.WriteLine("{0} Duplicates detected.", query.Sum(group => group.Count()));
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

        public void RemoveTransaction(Transaction transaction)
        {
            this.doNotUseAllTransactions.Remove(transaction);
            Filter(this.currentFilter);
        }

        /// <summary>
        ///     Used internally by the importers to load transactions into the statement model.
        /// </summary>
        /// <param name="transactions">The transactions to load.</param>
        /// <returns>Returns this instance, to allow chaining.</returns>
        internal virtual StatementModel LoadTransactions(IEnumerable<Transaction> transactions)
        {
            Transactions = transactions.OrderBy(t => t.Date).ToList();
            AllTransactions = Transactions;
            this.fullDuration = DurationInMonths;
            this.duplicates = null;
            AccountTypes = Transactions.Select(t => t.AccountType).Distinct().ToList();
            OnPropertyChanged("Transactions");
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
    }
}