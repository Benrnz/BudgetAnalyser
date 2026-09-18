using System.Collections.ObjectModel;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Transactions;

public partial class TransactionsListViewModel(IApplicationDatabaseFacade applicationDatabaseService, ITransactionManagerService transactionService)
    : ObservableRecipient
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
    private readonly ITransactionManagerService transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

    [ObservableProperty]
    public partial bool Dirty { get; set; }

    [ObservableProperty]
    public partial string? DuplicateSummary { get; private set; }

    public IEnumerable<string> FilterBudgetBuckets => this.transactionService.FilterableBuckets();

    public bool HasTransactions => TransactionsList is not null && TransactionsList.Transactions.Any();

    [ObservableProperty]
    public partial ObservableCollection<Transaction> PagedTransactions { get; internal set; } = new();

    [ObservableProperty]
    public partial Transaction? SelectedRow { get; set; }

    public decimal TotalCount => this.transactionService.TotalCount;
    public decimal TotalCredits => this.transactionService.TotalCredits;
    public decimal TotalDebits => this.transactionService.TotalDebits;
    public decimal TotalDifference => TotalCredits + TotalDebits;

    public string TransactionListModelName => TransactionsList is not null ? Path.GetFileNameWithoutExtension(TransactionsList.StorageKey) : "[No Transactions Loaded]";

    [ObservableProperty]
    public partial List<Transaction> Transactions { get; internal set; } = new();

    [ObservableProperty]
    public partial TransactionsListModel? TransactionsList { get; set; }

    public bool HasSelectedRow()
    {
        return SelectedRow is not null;
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
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(HasTransactions));
        OnPropertyChanged(nameof(TransactionListModelName));

        DuplicateSummary = TransactionsList is null ? null : this.transactionService.DetectDuplicateTransactions();
    }

    partial void OnDirtyChanged(bool value)
    {
        if (value)
        {
            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
        }
    }

    partial void OnTransactionsListChanged(TransactionsListModel? value)
    {
        Transactions = this.transactionService.ClearBucketAndTextFilters();
    }
}
