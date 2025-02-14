﻿using System.Text;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class ApplicationDatabaseService : IApplicationDatabaseService
{
    private readonly IApplicationDatabaseRepository applicationRepository;
    private readonly ICredentialStore credentialStore;
    private readonly IEnumerable<ISupportsModelPersistence> databaseDependents;
    private readonly IDirtyDataService dirtyDataService;
    private readonly ILogger logger;
    private readonly IMonitorableDependencies monitorableDependencies;

    private ApplicationDatabase? budgetAnalyserDatabase;

    public ApplicationDatabaseService(
        IApplicationDatabaseRepository applicationRepository,
        IEnumerable<ISupportsModelPersistence> databaseDependents,
        IMonitorableDependencies monitorableDependencies,
        ICredentialStore credentialStore,
        ILogger logger,
        IDirtyDataService dirtyDataService)
    {
        if (databaseDependents is null)
        {
            throw new ArgumentNullException(nameof(databaseDependents));
        }

        this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        this.monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
        this.credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.dirtyDataService = dirtyDataService ?? throw new ArgumentNullException(nameof(dirtyDataService));
        this.databaseDependents = databaseDependents.OrderBy(d => d.LoadSequence).ToList();
        this.monitorableDependencies.NotifyOfDependencyChange<IApplicationDatabaseService>(this);
    }

    public bool HasUnsavedChanges => this.dirtyDataService.HasUnsavedChanges;

    /// <inheritdoc />
    public bool IsEncrypted => this.budgetAnalyserDatabase is not null && this.budgetAnalyserDatabase.IsEncrypted;

    /// <inheritdoc />
    public void SetCredential(object? claim)
    {
        this.credentialStore.SetPasskey(claim);
    }

    /// <inheritdoc />
    public ApplicationDatabase? Close()
    {
        if (this.budgetAnalyserDatabase is null)
        {
            return null;
        }

        this.budgetAnalyserDatabase.LedgerReconciliationToDoCollection.Clear();
        // Only clears system generated tasks, not persistent user created tasks.
        foreach (var service in this.databaseDependents.OrderByDescending(d => d.LoadSequence))
        {
            service.Close();
        }

        this.dirtyDataService.ClearAllDirtyDataFlags();

        this.budgetAnalyserDatabase.Close();
        this.monitorableDependencies.NotifyOfDependencyChange<IApplicationDatabaseService>(this);
        this.monitorableDependencies.NotifyOfDependencyChange(this.budgetAnalyserDatabase);
        return this.budgetAnalyserDatabase;
    }

    /// <inheritdoc />
    public async Task<ApplicationDatabase> CreateNewDatabaseAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        this.dirtyDataService.ClearAllDirtyDataFlags();
        this.budgetAnalyserDatabase = await this.applicationRepository.CreateNewAsync(storageKey);
        foreach (var service in this.databaseDependents)
        {
            await service.CreateNewAsync(this.budgetAnalyserDatabase);
        }

        this.monitorableDependencies.NotifyOfDependencyChange(this.budgetAnalyserDatabase);
        return this.budgetAnalyserDatabase;
    }

    /// <inheritdoc />
    public async Task EncryptFilesAsync()
    {
        if (this.budgetAnalyserDatabase is null)
        {
            throw new ArgumentException("There is no Budget Analyser files loaded");
        }

        if (this.credentialStore.RetrievePasskey() is null)
        {
            throw new EncryptionKeyNotProvidedException("Attempt to use encryption but no password is set.");
        }

        await CreateBackup(); // Ensure data is not corrupted and lost when encrypting files

        this.dirtyDataService.SetAllDirtyFlags(); // Ensure all files are marked as requiring a save.
        this.budgetAnalyserDatabase.IsEncrypted = true;
        await SaveAsync();
    }

    /// <inheritdoc />
    public async Task DecryptFilesAsync(object confirmCredentialsClaim)
    {
        if (this.budgetAnalyserDatabase is null)
        {
            throw new ArgumentException("There is no Budget Analyser files loaded");
        }

        if (this.credentialStore.RetrievePasskey() is null)
        {
            throw new EncryptionKeyNotProvidedException("Attempt to use encryption but no password is set.");
        }

        if (!this.credentialStore.AreEqual(confirmCredentialsClaim))
        {
            throw new EncryptionKeyIncorrectException("The provided credential does not match the existing credential used to load the encrypted files.");
        }

        await CreateBackup(); // Ensure data is not corrupted and lost when encrypting files

        this.dirtyDataService.SetAllDirtyFlags(); // Ensure all files are marked as requiring a save.
        this.budgetAnalyserDatabase.IsEncrypted = false;

        await SaveAsync();

        // If the files are now unprotected (unencrypted) then ensure the password is no longer stored in memory.
        SetCredential(null);
    }

    /// <inheritdoc />
    public async Task<ApplicationDatabase> LoadAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        this.dirtyDataService.ClearAllDirtyDataFlags();
        var encryptionKey = this.credentialStore.RetrievePasskey();
        this.budgetAnalyserDatabase = await this.applicationRepository.LoadAsync(storageKey);
        if (this.budgetAnalyserDatabase.IsEncrypted && encryptionKey is null)
        {
            throw new EncryptionKeyNotProvidedException($"{this.budgetAnalyserDatabase.FileName} is encrypted and no password has been provided.");
        }

        try
        {
            foreach (var service in this.databaseDependents) // Already sorted ascending by sequence number.
            {
                this.logger.LogInfo(_ => $"Loading service: {service}");
                await service.LoadAsync(this.budgetAnalyserDatabase);
            }
        }
        catch (DataFormatException ex)
        {
            Close();
            throw new DataFormatException("A subordindate data file is invalid or corrupt unable to load " + storageKey, ex);
        }
        catch (KeyNotFoundException ex)
        {
            Close();
            throw new KeyNotFoundException("A subordinate data file cannot be found: " + ex.Message, ex);
        }
        catch (NotSupportedException ex)
        {
            Close();
            throw new DataFormatException("A subordinate data file contains unsupported data.", ex);
        }

        this.monitorableDependencies.NotifyOfDependencyChange(this.budgetAnalyserDatabase);
        this.monitorableDependencies.NotifyOfDependencyChange<IApplicationDatabaseService>(this);
        return this.budgetAnalyserDatabase;
    }

    /// <inheritdoc />
    public void NotifyOfChange(ApplicationDataType dataType)
    {
        this.dirtyDataService.NotifyOfChange(dataType);
        this.monitorableDependencies.NotifyOfDependencyChange<IApplicationDatabaseService>(this);
    }

    /// <inheritdoc />
    public MainApplicationState PreparePersistentStateData()
    {
        if (this.budgetAnalyserDatabase is null)
        {
            throw new ArgumentException("There is no budget analyser files loaded.");
        }

        if (string.IsNullOrWhiteSpace(this.budgetAnalyserDatabase.FileName))
        {
            throw new ArgumentException("The loaded budget analyser files have not been given a file name. Code error");
        }

        return new MainApplicationState { BudgetAnalyserDataStorageKey = this.budgetAnalyserDatabase.FileName };
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        if (this.budgetAnalyserDatabase is null)
        {
            throw new InvalidOperationException("Application Database cannot be null here. Code Bug.");
        }

        if (!HasUnsavedChanges)
        {
            return;
        }

        var messages = new StringBuilder();
        if (!ValidateAll(messages))
        {
            throw new ValidationWarningException(messages.ToString());
        }

        this.databaseDependents
            .Where(service => this.dirtyDataService.IsDirty(service.DataType))
            .ToList()
            .ForEach(service => service.SavePreview());

        // This clears all the temporary tasks from the collection.  Only tasks that have CanDelete=false will be kept and saved.
        this.budgetAnalyserDatabase.LedgerReconciliationToDoCollection.Clear();

        // Save the main application repository first.
        await this.applicationRepository.SaveAsync(this.budgetAnalyserDatabase);

        // Save all remaining service's data.
        foreach (var service in this.databaseDependents.Where(s => this.dirtyDataService.IsDirty(s.DataType)))
        {
            await service.SaveAsync(this.budgetAnalyserDatabase);
        }

        this.dirtyDataService.ClearAllDirtyDataFlags();
        this.monitorableDependencies.NotifyOfDependencyChange<IApplicationDatabaseService>(this);
    }

    /// <inheritdoc />
    public bool ValidateAll(StringBuilder messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var valid = true;
        foreach (var service in this.databaseDependents) // Already sorted ascending by sequence number.
        {
            try
            {
                valid &= service.ValidateModel(messages);
            }
            catch (ValidationWarningException ex)
            {
                messages.AppendLine(ex.Message);
                valid = false;
            }
        }

        return valid;
    }

    private async Task CreateBackup()
    {
        if (this.budgetAnalyserDatabase is null)
        {
            throw new ArgumentException("There is no budget analyser files loaded");
        }

        this.dirtyDataService.SetAllDirtyFlags();

        var backupSuffix = ".backup";
        var budgetStorageKey = this.budgetAnalyserDatabase.BudgetCollectionStorageKey;
        this.budgetAnalyserDatabase.BudgetCollectionStorageKey += backupSuffix;

        var ledgerStorageKey = this.budgetAnalyserDatabase.LedgerBookStorageKey;
        this.budgetAnalyserDatabase.LedgerBookStorageKey += backupSuffix;

        var matchingRuleStorageKey = this.budgetAnalyserDatabase.MatchingRulesCollectionStorageKey;
        this.budgetAnalyserDatabase.MatchingRulesCollectionStorageKey += backupSuffix;

        var statementStorageKey = this.budgetAnalyserDatabase.StatementModelStorageKey;
        this.budgetAnalyserDatabase.StatementModelStorageKey += backupSuffix;

        await SaveAsync();

        this.budgetAnalyserDatabase.BudgetCollectionStorageKey = budgetStorageKey;
        this.budgetAnalyserDatabase.LedgerBookStorageKey = ledgerStorageKey;
        this.budgetAnalyserDatabase.MatchingRulesCollectionStorageKey = matchingRuleStorageKey;
        this.budgetAnalyserDatabase.StatementModelStorageKey = statementStorageKey;
    }
}
