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
        private readonly IUiContext uiContext;
        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private ObservableCollection<TransactionGroupedByBucketViewModel> doNotUseGroupedByBucket;
        private Transaction doNotUseSelectedRow;
        private bool doNotUseSortByDate;
        private StatementModel doNotUseStatement;
        private ObservableCollection<Transaction> doNotUseTransactions;
        private ITransactionManagerService transactionService;

        public StatementViewModel(IUiContext uiContext)
        {
            this.doNotUseSortByDate = true;
            this.uiContext = uiContext;
        }

        public decimal AverageDebit
        {
            get { return this.transactionService.AverageDebit; }
        }

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
                if (this.transactionService == null)
                {
                    throw new InvalidOperationException("Initialise has not been called.");
                }

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
                Transactions = this.transactionService.ClearBucketAndTextFilters();
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
            get { return this.transactionService.TotalCount; }
        }

        public decimal TotalCredits
        {
            get { return this.transactionService.TotalCredits; }
        }

        public decimal TotalDebits
        {
            get { return this.transactionService.TotalDebits; }
        }

        public decimal TotalDifference
        {
            get { return TotalCredits + TotalDebits; }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get { return this.doNotUseTransactions; }
            internal set
            {
                this.doNotUseTransactions = value;
                RaisePropertyChanged();
            }
        }

        public bool HasSelectedRow()
        {
            return SelectedRow != null;
        }

        public StatementViewModel Initialise(ITransactionManagerService transactionManagerService)
        {
            this.transactionService = transactionManagerService;
            return this;
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
            GroupedByBucket = new ObservableCollection<TransactionGroupedByBucketViewModel>(
                this.transactionService.PopulateGroupByBucketCollection(SortByBucket)
                    .Select(x => new TransactionGroupedByBucketViewModel(x, this.uiContext)));
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