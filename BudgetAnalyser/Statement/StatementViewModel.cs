using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight;

namespace BudgetAnalyser.Statement
{
    public class StatementViewModel : ViewModelBase
    {
        private string doNotUseBucketFilter;
        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private ObservableCollection<TransactionGroupedByBucketViewModel> doNotUseGroupedByBucket;
        private Transaction doNotUseSelectedRow;
        private bool doNotUseSortByDate;
        private StatementModel doNotUseStatement;
        private StatementController statementController;
        private ITransactionManagerService transactionService;

        public StatementViewModel()
        {
            this.doNotUseSortByDate = true;
        }

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

                if (BucketFilter == TransactionManagerService.UncategorisedFilter)
                {
                    var query2 =
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

        /// <summary>
        ///     Gets or sets the bucket filter.
        ///     This is a string filter on the bucket code plus blank for all, and "[Uncatergorised]" for anything without a
        ///     bucket.
        ///     Only relevant when the view is displaying transactions by date.  The filter is hidden when shown in GroupByBucket
        ///     mode.
        /// </summary>
        public string BucketFilter
        {
            get { return this.doNotUseBucketFilter; }

            set
            {
                this.doNotUseBucketFilter = value;
                RaisePropertyChanged(() => BucketFilter);
                TriggerRefreshTotalsRow();
            }
        }

        // TODO Remove this
        //public BudgetModel BudgetModel { get; set; }

        public bool Dirty
        {
            get { return this.doNotUseDirty; }

            set
            {
                this.doNotUseDirty = value;
                RaisePropertyChanged(() => Dirty);
            }
        }

        public string DuplicateSummary
        {
            get { return this.doNotUseDuplicateSummary; }

            private set
            {
                this.doNotUseDuplicateSummary = value;
                RaisePropertyChanged(() => DuplicateSummary);
            }
        }

        public IEnumerable<string> FilterBudgetBuckets
        {
            get { return this.transactionService.FilterableBuckets(); }
        }

        public ObservableCollection<TransactionGroupedByBucketViewModel> GroupedByBucket
        {
            get { return this.doNotUseGroupedByBucket; }
            internal set
            {
                this.doNotUseGroupedByBucket = value;
                RaisePropertyChanged(() => GroupedByBucket);
            }
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

        public Transaction SelectedRow
        {
            get { return this.doNotUseSelectedRow; }
            set
            {
                this.doNotUseSelectedRow = value;
                RaisePropertyChanged(() => SelectedRow);
            }
        }

        public bool SortByBucket
        {
            get { return !this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = !value;
                RaisePropertyChanged(() => SortByDate);
                RaisePropertyChanged(() => SortByBucket);
            }
        }

        public bool SortByDate
        {
            get { return this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = value;
                RaisePropertyChanged(() => SortByBucket);
                RaisePropertyChanged(() => SortByDate);
            }
        }

        public StatementModel Statement
        {
            get { return this.doNotUseStatement; }

            set
            {
                if (this.doNotUseStatement != null)
                {
                    this.doNotUseStatement.PropertyChanged -= OnStatementPropertyChanged;
                }

                this.doNotUseStatement = value;

                if (this.doNotUseStatement != null)
                {
                    this.doNotUseStatement.PropertyChanged += OnStatementPropertyChanged;
                }

                RaisePropertyChanged(() => Statement);
                UpdateGroupedByBucket();
            }
        }

        public string StatementName
        {
            get
            {
                if (Statement != null)
                {
                    return Path.GetFileNameWithoutExtension(Statement.StorageKey);
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

                if (BucketFilter == TransactionManagerService.UncategorisedFilter)
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

                if (BucketFilter == TransactionManagerService.UncategorisedFilter)
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

                if (BucketFilter == TransactionManagerService.UncategorisedFilter)
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

        public bool HasSelectedRow()
        {
            return SelectedRow != null;
        }

        public StatementViewModel Initialise(StatementController controller, ITransactionManagerService transactionManagerService)
        {
            this.statementController = controller;
            this.transactionService = transactionManagerService;
            return this;
        }

        public void TriggerRefreshBucketFilter()
        {
            RaisePropertyChanged(() => BucketFilter);
        }

        public void TriggerRefreshBucketFilterList()
        {
            RaisePropertyChanged(() => FilterBudgetBuckets);
        }

        public void TriggerRefreshTotalsRow()
        {
            RaisePropertyChanged(() => TotalCredits);
            RaisePropertyChanged(() => TotalDebits);
            RaisePropertyChanged(() => TotalDifference);
            RaisePropertyChanged(() => AverageDebit);
            RaisePropertyChanged(() => TotalCount);
            RaisePropertyChanged(() => HasTransactions);
            RaisePropertyChanged(() => StatementName);
            RaisePropertyChanged(() => MinTransactionDate);
            RaisePropertyChanged(() => MaxTransactionDate);

            if (Statement == null)
            {
                DuplicateSummary = null;
            }
            else
            {
                DuplicateSummary = this.transactionService.DetectDuplicateTransactions();
            }
        }

        public void UpdateGroupedByBucket()
        {
            BucketFilter = string.Empty;
            GroupedByBucket = new ObservableCollection<TransactionGroupedByBucketViewModel>(
                this.transactionService.PopulateGroupByBucketCollection(SortByBucket)
                    .Select(x => new TransactionGroupedByBucketViewModel(x, this.statementController)));
        }

        private void OnStatementPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Caters for deleting a transaction. Could be more efficient if it becomes a problem.
            if (propertyChangedEventArgs.PropertyName == "Transactions")
            {
                UpdateGroupedByBucket();
            }
        }
    }
}