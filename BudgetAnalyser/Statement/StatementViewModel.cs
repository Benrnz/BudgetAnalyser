using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Statement;

public class StatementViewModel : ObservableRecipient
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

    public decimal AverageDebit => this.transactionService.AverageDebit;

    public bool Dirty
    {
        get => this.doNotUseDirty;

        set
        {
            this.doNotUseDirty = value;
            OnPropertyChanged();
            if (Dirty)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
            }
        }
    }

    public string DuplicateSummary
    {
        [UsedImplicitly] get => this.doNotUseDuplicateSummary;

        private set
        {
            this.doNotUseDuplicateSummary = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<string> FilterBudgetBuckets => this.transactionService.FilterableBuckets();

    public ObservableCollection<TransactionGroupedByBucketViewModel> GroupedByBucket
    {
        get => this.doNotUseGroupedByBucket;
        internal set
        {
            this.doNotUseGroupedByBucket = value;
            OnPropertyChanged();
        }
    }

    public bool HasTransactions => Statement != null && Statement.Transactions.Any();

    public Transaction SelectedRow
    {
        get => this.doNotUseSelectedRow;
        set
        {
            this.doNotUseSelectedRow = value;
            OnPropertyChanged();
        }
    }

    public bool SortByBucket
    {
        get => !this.doNotUseSortByDate;
        set
        {
            this.doNotUseSortByDate = !value;
            OnPropertyChanged(nameof(SortByDate));
            OnPropertyChanged();
        }
    }

    public bool SortByDate
    {
        get => this.doNotUseSortByDate;
        set
        {
            this.doNotUseSortByDate = value;
            OnPropertyChanged(nameof(SortByBucket));
            OnPropertyChanged();
        }
    }

    public StatementModel Statement
    {
        get => this.doNotUseStatement;

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

            OnPropertyChanged(nameof(Statement));
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

    public decimal TotalCount => this.transactionService.TotalCount;
    public decimal TotalCredits => this.transactionService.TotalCredits;
    public decimal TotalDebits => this.transactionService.TotalDebits;
    public decimal TotalDifference => TotalCredits + TotalDebits;

    public ObservableCollection<Transaction> Transactions
    {
        get => this.doNotUseTransactions;
        internal set
        {
            this.doNotUseTransactions = value;
            OnPropertyChanged();
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
        OnPropertyChanged(nameof(FilterBudgetBuckets));
    }

    public void TriggerRefreshTotalsRow()
    {
        OnPropertyChanged(nameof(TotalCredits));
        OnPropertyChanged(nameof(TotalDebits));
        OnPropertyChanged(nameof(TotalDifference));
        OnPropertyChanged(nameof(AverageDebit));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasTransactions));
        OnPropertyChanged(nameof(StatementName));

        DuplicateSummary = Statement == null ? null : this.transactionService.DetectDuplicateTransactions();
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