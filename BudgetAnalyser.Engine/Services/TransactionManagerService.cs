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
    private readonly ITransactionSetRepository _transactionSetRepository;
    private readonly IBudgetBucketRepository bucketRepository;
    private readonly ILogger logger;
    private readonly IMonitorableDependencies monitorableDependencies;
    private BudgetCollection? budgetCollection;
    private int budgetHash;
    private List<Transaction> transactions = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionManagerService" /> class.
    /// </summary>
    /// <param name="bucketRepository">The bucket repository.</param>
    /// <param name="transactionSetRepository">The transactions model repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="monitorableDependencies">The dependency monitor manager</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public TransactionManagerService(IBudgetBucketRepository bucketRepository, ITransactionSetRepository transactionSetRepository, ILogger logger, IMonitorableDependencies monitorableDependencies)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this._transactionSetRepository = transactionSetRepository ?? throw new ArgumentNullException(nameof(transactionSetRepository));
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
        this.transactions = new List<Transaction>();
        TransactionSetModel?.Dispose();
        TransactionSetModel = null;
        this.budgetCollection = null;
        this.budgetHash = 0;
        var handler = Closed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase.TransactionsSetModelStorageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        await this._transactionSetRepository.CreateNewAndSaveAsync(applicationDatabase.TransactionsSetModelStorageKey);
        await LoadAsync(applicationDatabase);
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        TransactionSetModel?.Dispose();
        try
        {
            TransactionSetModel = await this._transactionSetRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.TransactionsSetModelStorageKey), applicationDatabase.IsEncrypted);
        }
        catch (TransactionsModelChecksumException ex)
        {
            throw new DataFormatException("Transactions Model data is corrupt and has been tampered with. Unable to load.", ex);
        }

        NewDataAvailable();
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        if (TransactionSetModel is null)
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

        TransactionSetModel.StorageKey = applicationDatabase.FullPath(applicationDatabase.TransactionsSetModelStorageKey);
        await this._transactionSetRepository.SaveAsync(TransactionSetModel, applicationDatabase.IsEncrypted);
        this.monitorableDependencies.NotifyOfDependencyChange(TransactionSetModel);
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
    public TransactionSetModel? TransactionSetModel { get; private set; }

    /// <inheritdoc />
    public decimal TotalCount => this.transactions.None() ? 0 : this.transactions.Count();

    /// <inheritdoc />
    public decimal TotalCredits => this.transactions.None() ? 0 : this.transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

    /// <inheritdoc />
    public decimal TotalDebits => this.transactions.None() ? 0 : this.transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);

    /// <inheritdoc />
    public List<Transaction> ClearBucketAndTextFilters()
    {
        ResetTransactionsCollection();
        return this.transactions;
    }

    /// <inheritdoc />
    public string DetectDuplicateTransactions()
    {
        if (TransactionSetModel is null)
        {
            return string.Empty;
        }

        var duplicates = TransactionSetModel.ValidateAgainstDuplicates().ToList();
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
    public List<Transaction> FilterByBucket(string? bucketCode)
    {
        if (TransactionSetModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (bucketCode == TransactionConstants.UncategorisedFilter)
        {
            return this.transactions = new List<Transaction>(TransactionSetModel.Transactions.Where(t => t.BudgetBucket is null));
        }

        var bucket = bucketCode is null ? null : this.bucketRepository.GetByCode(bucketCode);

        if (bucket is null)
        {
            return new List<Transaction>(TransactionSetModel.Transactions);
        }

        var paternityTest = new BudgetBucketPaternity();
        return this.transactions = new List<Transaction>(TransactionSetModel.Transactions.Where(t => paternityTest.OfSameBucketFamily(t.BudgetBucket, bucket)));
    }

    /// <inheritdoc />
    public List<Transaction> FilterBySearchText(string? searchText)
    {
        if (TransactionSetModel is null)
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

        this.transactions = new List<Transaction>(TransactionSetModel.Transactions
            .Where(t => MatchTransactionText(t, searchText))
            .AsParallel()
            .ToList());
        return this.transactions;
    }

    /// <inheritdoc />
    public void FilterTransactions(GlobalFilterCriteria criteria)
    {
        if (TransactionSetModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (criteria is null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        this.monitorableDependencies.NotifyOfDependencyChange(criteria);
        TransactionSetModel.Filter(criteria);
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

        if (TransactionSetModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        var additionalModel = await this._transactionSetRepository.ImportBankStatementAsync(storageKey, account);
        var combinedModel = TransactionSetModel.Merge(additionalModel);
        var minDate = additionalModel.AllTransactions.Min(t => t.Date);
        var maxDate = additionalModel.AllTransactions.Max(t => t.Date);
        IEnumerable<IGrouping<int, Transaction>> duplicates = combinedModel.ValidateAgainstDuplicates(minDate, maxDate).ToList();
        if (duplicates.Count() == additionalModel.AllTransactions.Count())
        {
            throw new TransactionsAlreadyImportedException();
        }

        TransactionSetModel.Dispose();
        TransactionSetModel = combinedModel;
        NewDataAvailable();
    }

    /// <inheritdoc />
    public void RemoveTransaction(Transaction transactionToRemove)
    {
        if (TransactionSetModel is null)
        {
            throw new InvalidOperationException("There are no transactions loaded, you must first load an existing file or create a new one.");
        }

        if (transactionToRemove is null)
        {
            throw new ArgumentNullException(nameof(transactionToRemove));
        }

        TransactionSetModel.RemoveTransaction(transactionToRemove);
    }

    /// <inheritdoc />
    public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
    {
        if (TransactionSetModel is null)
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

        TransactionSetModel.SplitTransaction(
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

        if (TransactionSetModel is null || this.budgetCollection is null)
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
                return TransactionSetModel.AllTransactions
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
        this.monitorableDependencies.NotifyOfDependencyChange(TransactionSetModel);
        NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
    }

    private void ResetTransactionsCollection()
    {
        this.transactions = TransactionSetModel is null ? new List<Transaction>() : new List<Transaction>(TransactionSetModel.Transactions);
    }
}
