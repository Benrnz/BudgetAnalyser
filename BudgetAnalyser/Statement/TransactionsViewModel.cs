using System.Collections.ObjectModel;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Statement;

public class TransactionsViewModel : ObservableRecipient
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly ITransactionManagerService transactionService;
    private bool doNotUseDirty;
    private string? doNotUseDuplicateSummary;
    private ObservableCollection<Transaction> doNotUsePagedTransactions = new();
    private Transaction? doNotUseSelectedRow;
    private List<Transaction> doNotUseTransactions = new();
    private TransactionSetModel? doNotUseTransactionsModel;

    public TransactionsViewModel(IApplicationDatabaseFacade applicationDatabaseService, ITransactionManagerService transactionService)
    {
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

    public bool Dirty
    {
        get => this.doNotUseDirty;

        set
        {
            this.doNotUseDirty = value;
            OnPropertyChanged();
            if (value)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
            }
        }
    }

    public string? DuplicateSummary
    {
        get => this.doNotUseDuplicateSummary;

        private set
        {
            this.doNotUseDuplicateSummary = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<string> FilterBudgetBuckets => this.transactionService.FilterableBuckets();

    public bool HasTransactions => TransactionsModel is not null && TransactionsModel.Transactions.Any();

    public ObservableCollection<Transaction> PagedTransactions
    {
        get => this.doNotUsePagedTransactions;
        internal set
        {
            if (Equals(value, this.doNotUsePagedTransactions))
            {
                return;
            }

            this.doNotUsePagedTransactions = value;
            OnPropertyChanged();
        }
    }

    public Transaction? SelectedRow
    {
        get => this.doNotUseSelectedRow;
        set
        {
            this.doNotUseSelectedRow = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalCount => this.transactionService.TotalCount;
    public decimal TotalCredits => this.transactionService.TotalCredits;
    public decimal TotalDebits => this.transactionService.TotalDebits;
    public decimal TotalDifference => TotalCredits + TotalDebits;

    public List<Transaction> Transactions
    {
        get => this.doNotUseTransactions;
        internal set
        {
            this.doNotUseTransactions = value;
            OnPropertyChanged();
        }
    }

    public TransactionSetModel? TransactionsModel
    {
        get => this.doNotUseTransactionsModel;

        set
        {
            this.doNotUseTransactionsModel = value;

            OnPropertyChanged();
            Transactions = this.transactionService.ClearBucketAndTextFilters();
        }
    }

    public string TransactionsModelName => TransactionsModel is not null ? Path.GetFileNameWithoutExtension(TransactionsModel.StorageKey) : "[No Transactions Loaded]";

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
        OnPropertyChanged(nameof(TransactionsModelName));

        DuplicateSummary = TransactionsModel is null ? null : this.transactionService.DetectDuplicateTransactions();
    }
}
