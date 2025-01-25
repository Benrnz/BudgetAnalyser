using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
[UsedImplicitly] // Used by IoC
internal class LedgerService(
    ILedgerBookRepository ledgerRepository,
    IAccountTypeRepository accountTypeRepository,
    ILedgerBucketFactory ledgerBucketFactory,
    MonitorableDependencies monitorableDependencies)
    : ILedgerService, ISupportsModelPersistence
{
    private readonly IAccountTypeRepository accountTypeRepository = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));
    private readonly ILedgerBucketFactory ledgerBucketFactory = ledgerBucketFactory ?? throw new ArgumentNullException(nameof(ledgerBucketFactory));
    private readonly ILedgerBookRepository ledgerRepository = ledgerRepository ?? throw new ArgumentNullException(nameof(ledgerRepository));
    private readonly MonitorableDependencies monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));

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
    public LedgerBook? LedgerBook { get; private set; }

    /// <inheritdoc />
    public void MoveLedgerToAccount(LedgerBucket ledger, Account storedInAccount)
    {
        if (ledger is null)
        {
            throw new ArgumentNullException(nameof(ledger));
        }

        if (storedInAccount is null)
        {
            throw new ArgumentNullException(nameof(storedInAccount));
        }

        if (LedgerBook is null)
        {
            throw new InvalidOperationException("Ledger Book file has not been loaded.");
        }

        LedgerBook.SetLedgerAccount(ledger, storedInAccount);
    }

    /// <inheritdoc />
    public void RenameLedgerBook(string newName)
    {
        if (LedgerBook is null)
        {
            throw new InvalidOperationException("Ledger Book file has not been loaded.");
        }

        if (newName is null)
        {
            throw new ArgumentNullException(nameof(newName));
        }

        LedgerBook.Name = newName;
    }

    /// <inheritdoc />
    public void TrackNewBudgetBucket(ExpenseBucket bucket, Account storeInThisAccount)
    {
        if (LedgerBook is null)
        {
            throw new InvalidOperationException("Ledger Book file has not been loaded.");
        }

        if (bucket is null)
        {
            throw new ArgumentNullException(nameof(bucket));
        }

        if (storeInThisAccount is null)
        {
            throw new ArgumentNullException(nameof(storeInThisAccount));
        }

        var newLedger = this.ledgerBucketFactory.Build(bucket.Code, storeInThisAccount);
        LedgerBook.AddLedger(newLedger);
    }

    /// <inheritdoc />
    public IEnumerable<Account> ValidLedgerAccounts()
    {
        return this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
    }

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.Ledger;

    /// <inheritdoc />
    public int LoadSequence => 50;

    /// <inheritdoc />
    public void Close()
    {
        LedgerBook = null;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CreateAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase.LedgerBookStorageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        await this.ledgerRepository.CreateNewAndSaveAsync(applicationDatabase.LedgerBookStorageKey);
        await LoadAsync(applicationDatabase);
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        LedgerBook = await this.ledgerRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey), applicationDatabase.IsEncrypted);

        this.monitorableDependencies.NotifyOfDependencyChange(LedgerBook);
        NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        if (LedgerBook is null)
        {
            throw new InvalidOperationException("Ledger Book file has not been loaded.");
        }

        Saving?.Invoke(this, new ValidatingEventArgs());

        var messages = new StringBuilder();
        if (!LedgerBook.Validate(messages))
        {
            throw new ValidationWarningException("Ledger Book is invalid, cannot save at this time:\n" + messages);
        }

        await this.ledgerRepository.SaveAsync(LedgerBook, applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey), applicationDatabase.IsEncrypted);
        this.monitorableDependencies.NotifyOfDependencyChange(LedgerBook);
        Saved?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SavePreview()
    {
    }

    /// <inheritdoc />
    public bool ValidateModel(StringBuilder messages)
    {
        if (LedgerBook is null)
        {
            throw new InvalidOperationException("Ledger Book file has not been loaded.");
        }

        var handler = Validating;
        handler?.Invoke(this, new ValidatingEventArgs());

        return LedgerBook.Validate(messages);
    }
}
