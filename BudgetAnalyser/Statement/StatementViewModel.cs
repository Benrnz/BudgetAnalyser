using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Statement
{
    public class StatementViewModel : INotifyPropertyChanged
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository budgetBucketRepository;

        private string doNotUseBucketFilter;
        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private StatementModel doNotUseStatement;

        public StatementViewModel([NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.budgetBucketRepository = budgetBucketRepository;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public decimal AverageDebit
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    IEnumerable<Transaction> query = Statement.Transactions.Where(t => t.Amount < 0).ToList();
                    if (query.Any())
                    {
                        return query.Average(t => t.Amount);
                    }
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    List<Transaction> query2 =
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code)).ToList();
                    if (query2.Any())
                    {
                        return query2.Average(t => t.Amount);
                    }

                    return 0;
                }

                IEnumerable<Transaction> query3 = Statement.Transactions
                    .Where(
                        t =>
                            t.Amount < 0 && t.BudgetBucket != null &&
                            t.BudgetBucket.Code == BucketFilter)
                    .ToList();
                if (query3.Any())
                {
                    return query3.Average(t => t.Amount);
                }

                return 0;
            }
        }

        public string BucketFilter
        {
            get { return this.doNotUseBucketFilter; }

            set
            {
                // TODO Change to a multi-select drop down and allow one or many buckets to be selected.
                this.doNotUseBucketFilter = value;
                OnPropertyChanged();
                TriggerRefreshTotalsRow();
            }
        }

        public IEnumerable<string> BudgetBuckets
        {
            get
            {
                return this.budgetBucketRepository.Buckets
                    .Select(b => b.Code)
                    .Union(new[] { string.Empty }).OrderBy(b => b);
            }
        }

        public BudgetModel BudgetModel { get; set; }

        public bool Dirty
        {
            get { return this.doNotUseDirty; }

            set
            {
                this.doNotUseDirty = value;
                OnPropertyChanged();
            }
        }

        public string DuplicateSummary
        {
            get { return this.doNotUseDuplicateSummary; }

            private set
            {
                this.doNotUseDuplicateSummary = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> FilterBudgetBuckets
        {
            get { return BudgetBuckets.Union(new[] { UncategorisedFilter }).OrderBy(b => b); }
        }

        public bool HasTransactions
        {
            get { return Statement != null && Statement.Transactions.Any(); }
        }

        public DateTime MaxTransactionDate
        {
            get { return Statement.Transactions.Max(t => t.Date); }
        }

        public DateTime MinTransactionDate
        {
            get { return Statement.Transactions.Min(t => t.Date); }
        }

        public StatementModel Statement
        {
            get { return this.doNotUseStatement; }

            set
            {
                this.doNotUseStatement = value;
                OnPropertyChanged();
            }
        }

        public string StatementName
        {
            get
            {
                if (Statement != null)
                {
                    return Path.GetFileNameWithoutExtension(Statement.FileName);
                }

                return "[No Transactions Loaded]";
            }
        }

        public decimal TotalCount
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Count();
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Count(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code));
                }

                return Statement.Transactions.Count(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter);
            }
        }

        public decimal TotalCredits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount > 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDebits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount < 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDifference
        {
            get { return TotalCredits + TotalDebits; }
        }

        public void TriggerRefreshTotalsRow()
        {
            OnPropertyChanged("TotalCredits");
            OnPropertyChanged("TotalDebits");
            OnPropertyChanged("TotalDifference");
            OnPropertyChanged("AverageDebit");
            OnPropertyChanged("TotalCount");
            OnPropertyChanged("HasTransactions");
            OnPropertyChanged("StatementName");

            if (Statement == null)
            {
                DuplicateSummary = null;
            }
            else
            {
                List<IGrouping<int, Transaction>> duplicates = Statement.ValidateAgainstDuplicates().ToList();
                DuplicateSummary = duplicates.Any()
                    ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!",
                        duplicates.Sum(group => group.Count()))
                    : null;
            }
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