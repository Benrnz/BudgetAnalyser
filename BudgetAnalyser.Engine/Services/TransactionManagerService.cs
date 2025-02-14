using System.Collections.ObjectModel;
using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service to manipulate and manage transactions and statements.
/// </summary>
/// <seealso cref="ITransactionManagerService" />
/// <seealso cref="ISupportsModelPersistence" />
[AutoRegisterWithIoC(SingleInstance = true)]
internal class TransactionManagerService : ITransactionManagerService, ISupportsModelPersistence
{
    private readonly IBudgetBucketRepository bucketRepository;
    private readonly ILogger logger;
    private readonly IMonitorableDependencies monitorableDependencies;
    private readonly IStatementRepository statementRepository;
    private BudgetCollection? budgetCollection;
    private int budgetHash;
    private bool sortedByBucket;
    private ObservableCollection<Transaction> transactions = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionManagerService" /> class.
    /// </summary>
    /// <param name="bucketRepository">The bucket repository.</param>
    /// <param name="statementRepository">The statement repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="monitorableDependencies">The dependency monitor manager</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public TransactionManagerService(IBudgetBucketRepository bucketRepository, IStatementRepository statementRepository, ILogger logger, IMonitorableDependencies monitorableDependencies)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.statementRepository = statementRepository ?? throw new ArgumentNullException(nameof(statementRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
    }

    /// <inheritdoc />
    public event EventHandler? Closed;

    /// <inheritdoc />
    public event EventHandler? NewDataSourceAvailable;

    /// <inheritdoc />
    public event EventHandler? Saved;

    /// <inheritdoc />
    public event EventHandler<ValidatingEventArgs>? Saving;

    /// <inheritdoc />
    public event EventHandler<ValidatingEventArgs>? Validating;

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.Transactions;

    /// <inheritdoc />
    public int LoadSequence => 10;

    /// <inheritdoc />
    public void Close()
    {
        this.transactions = new ObservableCollection<Transaction>();
        StatementModel?.Dispose();
        StatementModel = null;
        this.budgetCollection = null;
        this.budgetHash = 0;
        var handler = Closed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase.StatementModelStorageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        await this.statementRepository.CreateNewAndSaveAsync(applicationDatabase.StatementModelStorageKey);
        await LoadAsync(applicationDatabase);
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        StatementModel?.Dispose();
        try
        {
            StatementModel = await this.statementRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.StatementModelStorageKey), applicationDatabase.IsEncrypted);
        }
        catch (StatementModelChecksumException ex)
        {
            throw new DataFormatException("Statement Model data is corrupt and has been tampered with. Unable to load.", ex);
        }

        NewDataAvailable();
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        if (StatementModel is null)
        {
            return;
        }

        var handler = Saving;
        handler?.Invoke(this, new ValidatingEventArgs());

        var messages = new StringBuilder();
        if (!ValidateModel(messages))
        {
            throw new ValidationWarningException("Unable to save transactions at this time, some data is invalid. " + messages);
        }

        StatementModel.StorageKey = applicationDatabase.FullPath(applicationDatabase.StatementModelStorageKey);
        await this.statementRepository.SaveAsync(StatementModel, applicationDatabase.IsEncrypted);
        this.monitorableDependencies.NotifyOfDependencyChange(StatementModel);
        Saved?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SavePreview()
    {
    }

    /// <inheritdoc />
    public bool ValidateModel(StringBuilder messages)
    {
        Validating?.Invoke(this, new ValidatingEventArgs());

        // In the case of the StatementModel all edits are validated and resolved during data edits. No need for an overall consistency check.
        return true;
    }

    /// <inheritdoc />
    public decimal AverageDebit => this.transactions.None() ? 0 : this.transactions.Where(t => t.Amount < 0).SafeAverage(t => t.Amount);

    /// <inheritdoc />
    public StatementModel? StatementModel { get; private set; }

    /// <inheritdoc />
    public decimal TotalCount => this.transactions.None() ? 0 : this.transactions.Count();

    /// <inheritdoc />
    public decimal TotalCredits => this.transactions.None() ? 0 : this.transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

    /// <inheritdoc />
    public decimal TotalDebits => this.transactions.None() ? 0 : this.transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);

    /// <inheritdoc />
    public ObservableCollection<Transaction> ClearBucketAndTextFilters()
    {
        ResetTransactionsCollection();
        return this.transactions;
    }

    /// <inheritdoc />
    public string DetectDuplicateTransactions()
    {
        if (StatementModel is null)
        {
            return string.Empty;
        }

        var duplicates = StatementModel.ValidateAgainstDuplicates().ToList();
        return duplicates.Any() ? $"{duplicates.Sum(group => group.Count())} suspected duplicates!" : string.Empty;
    }

    /// <inheritdoc />
    public IEnumerable<string> FilterableBuckets()
    {
        return this.bucketRepository.Buckets
            .Where(b => b.Active)
            .Select(b => b.Code)
            .Union([string.Empty, TransactionConstants.UncategorisedFilter])
            .OrderBy(b => b);
    }

    /// <inheritdoc />
    public ObservableCollection<Transaction> FilterByBucket(string? bucketCode)
    {
        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (bucketCode == TransactionConstants.UncategorisedFilter)
        {
            return this.transactions = new ObservableCollection<Transaction>(StatementModel.Transactions.Where(t => t.BudgetBucket is null));
        }

        var bucket = bucketCode is null ? null : this.bucketRepository.GetByCode(bucketCode);

        if (bucket is null)
        {
            return new ObservableCollection<Transaction>(StatementModel.Transactions);
        }

        var paternityTest = new BudgetBucketPaternity();
        return this.transactions = new ObservableCollection<Transaction>(StatementModel.Transactions.Where(t => paternityTest.OfSameBucketFamily(t.BudgetBucket, bucket)));
    }

    /// <inheritdoc />
    public ObservableCollection<Transaction> FilterBySearchText(string? searchText)
    {
        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return ClearBucketAndTextFilters();
        }

        if (searchText.Length < 3)
        {
            return ClearBucketAndTextFilters();
        }

        this.transactions = new ObservableCollection<Transaction>(StatementModel.Transactions
            .Where(t => MatchTransactionText(t, searchText))
            .AsParallel()
            .ToList());
        return this.transactions;
    }

    /// <inheritdoc />
    public void FilterTransactions(GlobalFilterCriteria criteria)
    {
        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (criteria is null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        this.monitorableDependencies.NotifyOfDependencyChange(criteria);
        StatementModel.Filter(criteria);
    }

    /// <inheritdoc />
    public async Task ImportAndMergeBankStatementAsync(string storageKey, Account account)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        if (account is null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        var additionalModel = await this.statementRepository.ImportBankStatementAsync(storageKey, account);
        var combinedModel = StatementModel.Merge(additionalModel);
        var minDate = additionalModel.AllTransactions.Min(t => t.Date);
        var maxDate = additionalModel.AllTransactions.Max(t => t.Date);
        IEnumerable<IGrouping<int, Transaction>> duplicates = combinedModel.ValidateAgainstDuplicates(minDate, maxDate).ToList();
        if (duplicates.Count() == additionalModel.AllTransactions.Count())
        {
            throw new TransactionsAlreadyImportedException();
        }

        StatementModel.Dispose();
        StatementModel = combinedModel;
        NewDataAvailable();
    }

    /// <inheritdoc />
    public void Initialise(StatementApplicationState stateData)
    {
        if (stateData is null)
        {
            throw new ArgumentNullException(nameof(stateData));
        }

        this.budgetHash = 0;
        this.sortedByBucket = stateData.SortByBucket ?? false;
    }

    /// <inheritdoc />
    public StatementApplicationState PreparePersistentStateData()
    {
        return new StatementApplicationState { SortByBucket = this.sortedByBucket };
    }

    /// <inheritdoc />
    public void RemoveTransaction(Transaction transactionToRemove)
    {
        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (transactionToRemove is null)
        {
            throw new ArgumentNullException(nameof(transactionToRemove));
        }

        StatementModel.RemoveTransaction(transactionToRemove);
    }

    /// <inheritdoc />
    public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
    {
        if (StatementModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (originalTransaction is null)
        {
            throw new ArgumentNullException(nameof(originalTransaction));
        }

        if (splinterBucket1 is null)
        {
            throw new ArgumentNullException(nameof(splinterBucket1));
        }

        if (splinterBucket2 is null)
        {
            throw new ArgumentNullException(nameof(splinterBucket2));
        }

        StatementModel.SplitTransaction(
            originalTransaction,
            splinterAmount1,
            splinterAmount2,
            splinterBucket1,
            splinterBucket2);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateWithCurrentBudgetsAsync(BudgetCollection? budgets = null)
    {
        // This method must be called at least once with a budget collection.  Second and subsequent times do not require the budget.
        if (this.budgetCollection is null && budgets is null)
        {
            throw new ArgumentNullException(nameof(budgets));
        }

        this.budgetCollection = budgets ?? this.budgetCollection;

        if (StatementModel is null || this.budgetCollection is null)
        {
            // Can't check yet, statement hasn't been loaded yet. Everything is ok for now.
            return true;
        }

        if (this.budgetCollection.GetHashCode() == this.budgetHash)
        {
            // This budget has already been checked against this statement. No need to repeatedly check the validity below, this is an expensive operation.
            // Everything is ok.
            return true;
        }

        var allBuckets = new List<BudgetBucket>(this.bucketRepository.Buckets.OrderBy(b => b.Code));
        var allTransactionHaveABucket = await Task.Run(
            () =>
            {
                return StatementModel.AllTransactions
                    .Where(t => t.BudgetBucket is not null)
                    .AsParallel()
                    .All(
                        t =>
                        {
                            var bucketExists = allBuckets.Contains(t.BudgetBucket!);
                            if (!bucketExists)
                            {
                                t.BudgetBucket = null;
                                this.logger.LogWarning(l => l.Format("Transaction {0} has a bucket ({1}) that doesn't exist!", t.Date, t.BudgetBucket));
                            }

                            return bucketExists;
                        });
            });

        this.budgetHash = this.budgetCollection.GetHashCode();
        return allTransactionHaveABucket;
    }

    private static bool MatchTransactionText(Transaction t, string textFilter)
    {
        if (!string.IsNullOrWhiteSpace(t.Description))
        {
            if (t.Description.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(t.Reference1))
        {
            if (t.Reference1.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(t.Reference2))
        {
            if (t.Reference2.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(t.Reference3))
        {
            if (t.Reference3.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private void NewDataAvailable()
    {
        ResetTransactionsCollection();
        this.monitorableDependencies.NotifyOfDependencyChange(StatementModel);
        NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
    }

    private void ResetTransactionsCollection()
    {
        this.transactions = StatementModel is null ? new ObservableCollection<Transaction>() : new ObservableCollection<Transaction>(StatementModel.Transactions);
    }
}
