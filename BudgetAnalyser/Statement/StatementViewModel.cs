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
    private bool doNotUseDirty;
    private string? doNotUseDuplicateSummary;
    private Transaction? doNotUseSelectedRow;
    private StatementModel? doNotUseStatement;
    private ObservableCollection<Transaction> doNotUseTransactions = new();

    public StatementViewModel(IApplicationDatabaseFacade applicationDatabaseService, ITransactionManagerService transactionService)
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

    public bool HasTransactions => Statement is not null && Statement.Transactions.Any();

    public Transaction? SelectedRow
    {
        get => this.doNotUseSelectedRow;
        set
        {
            this.doNotUseSelectedRow = value;
            OnPropertyChanged();
        }
    }

    public StatementModel? Statement
    {
        get => this.doNotUseStatement;

        set
        {
            this.doNotUseStatement = value;

            OnPropertyChanged();
            Transactions = this.transactionService.ClearBucketAndTextFilters();
        }
    }

    public string StatementName => Statement is not null ? Path.GetFileNameWithoutExtension(Statement.StorageKey) : "[No Transactions Loaded]";

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
