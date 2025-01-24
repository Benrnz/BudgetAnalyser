using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service to provide maintenance support for budget models and collections. This class is stateful and is intended to be used as a single instance.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class BudgetMaintenanceService : IBudgetMaintenanceService, ISupportsModelPersistence
{
    private readonly IBudgetRepository budgetRepository;
    private readonly ILogger logger;
    private readonly MonitorableDependencies monitorableDependencies;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BudgetMaintenanceService" /> class.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">budgetRepository</exception>
    public BudgetMaintenanceService(
        IBudgetRepository budgetRepository,
        IBudgetBucketRepository bucketRepo,
        ILogger logger,
        MonitorableDependencies monitorableDependencies)
    {
        this.budgetRepository = budgetRepository ?? throw new ArgumentNullException(nameof(budgetRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
        BudgetBucketRepository = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        Budgets = this.budgetRepository.CreateNew();
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
    public IBudgetBucketRepository BudgetBucketRepository { get; }

    /// <inheritdoc />
    public BudgetCollection? Budgets { get; private set; }

    /// <inheritdoc />
    public BudgetModel CloneBudgetModel(BudgetModel sourceBudget, DateTime newBudgetEffectiveFrom, BudgetCycle budgetCycle)
    {
        if (sourceBudget is null)
        {
            throw new ArgumentNullException(nameof(sourceBudget));
        }

        if (newBudgetEffectiveFrom <= sourceBudget.EffectiveFrom)
        {
            throw new ArgumentException("The effective date of the new budget must be later than the other budget.", nameof(newBudgetEffectiveFrom));
        }

        if (newBudgetEffectiveFrom <= DateTime.Today)
        {
            throw new ArgumentException("The effective date of the new budget must be a future date.", nameof(newBudgetEffectiveFrom));
        }

        if (Budgets is null)
        {
            throw new InvalidOperationException("No Budget file is loaded.");
        }

        var validationMessages = new StringBuilder();
        if (!sourceBudget.Validate(validationMessages))
        {
            throw new ValidationWarningException(string.Format(CultureInfo.CurrentCulture, "The source budget is currently in an invalid state, unable to clone it at this time.\n{0}",
                validationMessages));
        }

        var newBudget = new BudgetModel { EffectiveFrom = newBudgetEffectiveFrom, Name = string.Format(CultureInfo.CurrentCulture, "Copy of {0}", sourceBudget.Name), BudgetCycle = budgetCycle };
        newBudget.Update(CloneBudgetIncomes(sourceBudget), CloneBudgetExpenses(sourceBudget));

        if (!newBudget.Validate(validationMessages))
        {
            throw new InvalidOperationException("New cloned budget is invalid but the source budget is ok. Code Error.\n" + validationMessages);
        }

        Budgets.Add(newBudget);
        this.budgetRepository.SaveAsync();
        UpdateServiceMonitor();
        return newBudget;
    }

    /// <inheritdoc />
    public void UpdateIncomesAndExpenses(BudgetModel model, IEnumerable<Income> allIncomes, IEnumerable<Expense> allExpenses)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        // Copy view model bound data back into model.
        model.Update(allIncomes, allExpenses);
    }

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.Budget;

    /// <inheritdoc />
    public int LoadSequence => 5;

    /// <inheritdoc />
    public void Close()
    {
        Budgets = null;
        var handler = Closed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CreateAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase.BudgetCollectionStorageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        await this.budgetRepository.CreateNewAndSaveAsync(applicationDatabase.BudgetCollectionStorageKey);
        await LoadAsync(applicationDatabase);
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        Budgets = await this.budgetRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.BudgetCollectionStorageKey), applicationDatabase.IsEncrypted);
        UpdateServiceMonitor();
        NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        if (Budgets is null)
        {
            throw new InvalidOperationException("No Budget file is loaded.");
        }

        EnsureAllBucketsUsedAreInBucketRepo();

        var messages = new StringBuilder();
        if (Budgets.Validate(messages))
        {
            await this.budgetRepository.SaveAsync(applicationDatabase.FullPath(applicationDatabase.BudgetCollectionStorageKey), applicationDatabase.IsEncrypted);
            var savedHandler = Saved;
            savedHandler?.Invoke(this, EventArgs.Empty);
            return;
        }

        this.logger.LogWarning(l => l.Format("BudgetMaintenanceService.Save: unable to save due to validation errors:\n{0}", messages));
        throw new ValidationWarningException("Unable to save Budget:\n" + messages);
    }

    /// <inheritdoc />
    public void SavePreview()
    {
        var args = new ValidatingEventArgs();
        Saving?.Invoke(this, args);
    }

    /// <inheritdoc />
    public bool ValidateModel(StringBuilder messages)
    {
        if (Budgets is null)
        {
            throw new InvalidOperationException("No Budget file is loaded.");
        }

        var handler = Validating;
        var args = new ValidatingEventArgs();
        handler?.Invoke(this, args);

        return Budgets.Validate(messages);
    }

    private static IEnumerable<Expense> CloneBudgetExpenses(BudgetModel source)
    {
        return source.Expenses.Select(
            sourceExpense => new Expense { Amount = sourceExpense.Amount, Bucket = sourceExpense.Bucket }).ToList();
    }

    private static IEnumerable<Income> CloneBudgetIncomes(BudgetModel source)
    {
        return source.Incomes.Select(
            sourceExpense => new Income { Amount = sourceExpense.Amount, Bucket = sourceExpense.Bucket }).ToList();
    }

    private void EnsureAllBucketsUsedAreInBucketRepo()
    {
        // Make sure all buckets are in the bucket repo.
        var buckets = Budgets!.SelectMany(b => b.Expenses.Select(e => e.Bucket))
            .Union(Budgets!.SelectMany(b => b.Incomes.Select(i => i.Bucket)))
            .Distinct();

        foreach (var budgetBucket in buckets)
        {
            var copyOfBucket = budgetBucket;
            BudgetBucketRepository.GetOrCreateNew(copyOfBucket.Code, () => copyOfBucket);
        }
    }

    private void UpdateServiceMonitor()
    {
        this.monitorableDependencies.NotifyOfDependencyChange(BudgetBucketRepository);
        var current = Budgets!.CurrentActiveBudget ?? Budgets.First();
        this.monitorableDependencies.NotifyOfDependencyChange<IBudgetCurrencyContext>(new BudgetCurrencyContext(Budgets, current));
        this.monitorableDependencies.NotifyOfDependencyChange(Budgets);
    }
}
