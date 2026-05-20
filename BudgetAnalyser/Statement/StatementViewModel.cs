using System.Collections.ObjectModel;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Statement;

public class StatementViewModel : ObservableRecipient
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly ITransactionManagerService transactionService;

    public StatementViewModel(IApplicationDatabaseFacade applicationDatabaseService, ITransactionManagerService transactionService)
    {
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

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

    public bool HasTransactions => Statement is not null && Statement.Transactions.Any();

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

    public StatementModel? Statement
    {
        get;

        set
        {
            field = value;

            OnPropertyChanged();
            Transactions = this.transactionService.ClearBucketAndTextFilters();
        }
    }

    public string StatementName => Statement is not null ? Path.GetFileNameWithoutExtension(Statement.StorageKey) : "[No Transactions Loaded]";

    public decimal TotalCount => this.transactionService.TotalCount;
    public decimal TotalCredits => this.transactionService.TotalCredits;
    public decimal TotalDebits => this.transactionService.TotalDebits;
    public decimal TotalDifference => TotalCredits + TotalDebits;

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
        OnPropertyChanged(nameof(StatementName));

        DuplicateSummary = Statement is null ? null : this.transactionService.DetectDuplicateTransactions();
    }
}
