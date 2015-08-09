using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight;

namespace BudgetAnalyser.Statement
{
    public class StatementViewModel : ViewModelBase
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly IUiContext uiContext;
        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private ObservableCollection<TransactionGroupedByBucketViewModel> doNotUseGroupedByBucket;
        private Transaction doNotUseSelectedRow;
        private bool doNotUseSortByDate;
        private StatementModel doNotUseStatement;
        private ObservableCollection<Transaction> doNotUseTransactions;
        private ITransactionManagerService transactionService;

        public StatementViewModel([NotNull] IUiContext uiContext, [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.doNotUseSortByDate = true;
            this.uiContext = uiContext;
            this.applicationDatabaseService = applicationDatabaseService;
        }

        public  decimal AverageDebit => this.transactionService.AverageDebit;

        public bool Dirty
        {
            get { return this.doNotUseDirty; }

            set
            {
                this.doNotUseDirty = value;
                RaisePropertyChanged();
                if (Dirty)
                {
                    this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
                }
            }
        }

        public string DuplicateSummary
        {
            get { return this.doNotUseDuplicateSummary; }

            private set
            {
                this.doNotUseDuplicateSummary = value;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public  bool HasTransactions => Statement != null && Statement.Transactions.Any();

        public Transaction SelectedRow
        {
            get { return this.doNotUseSelectedRow; }
            set
            {
                this.doNotUseSelectedRow = value;
                RaisePropertyChanged();
            }
        }

        public bool SortByBucket
        {
            get { return !this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = !value;
                RaisePropertyChanged(() => SortByDate);
                RaisePropertyChanged();
            }
        }

        public bool SortByDate
        {
            get { return this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = value;
                RaisePropertyChanged(() => SortByBucket);
                RaisePropertyChanged();
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

        public  decimal TotalCount => this.transactionService.TotalCount;

        public  decimal TotalCredits => this.transactionService.TotalCredits;

        public  decimal TotalDebits => this.transactionService.TotalDebits;

        public  decimal TotalDifference => TotalCredits + TotalDebits;

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