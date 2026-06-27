using System.Collections.ObjectModel;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Transactions;

public class TransactionsListViewModel(IApplicationDatabaseFacade applicationDatabaseService, ITransactionManagerService transactionService)
    : ObservableRecipient
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
    private readonly ITransactionManagerService transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

    public bool Dirty
    {
        get;

        set
        {
            field = value;
            OnPropertyChanged();
            if (value)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
            }
        }
    }

    public string? DuplicateSummary
    {
        get;

        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<string> FilterBudgetBuckets => this.transactionService.FilterableBuckets();

    public bool HasTransactions => TransactionsList is not null && TransactionsList.Transactions.Any();

    public ObservableCollection<Transaction> PagedTransactions
    {
        get;
        internal set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public Transaction? SelectedRow
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalCount => this.transactionService.TotalCount;
    public decimal TotalCredits => this.transactionService.TotalCredits;
    public decimal TotalDebits => this.transactionService.TotalDebits;
    public decimal TotalDifference => TotalCredits + TotalDebits;

    public string TransactionListModelName => TransactionsList is not null ? Path.GetFileNameWithoutExtension(TransactionsList.StorageKey) : "[No Transactions Loaded]";

    // TODO Does this need to be an ObservableCollection?
    public List<Transaction> Transactions
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public TransactionsListModel? TransactionsList
    {
        get;

        set
        {
            field = value;

            OnPropertyChanged();
            Transactions = this.transactionService.ClearBucketAndTextFilters();
        }
    }

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
}
