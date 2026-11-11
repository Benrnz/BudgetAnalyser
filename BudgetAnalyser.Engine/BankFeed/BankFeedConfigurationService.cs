using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Manages the bank feed credentials and account mappings, and decides whether the bank feed is ready to use. The account mappings are part of the main Budget Analyser file,
///     so this takes part in loading and closing that file. It has nothing of its own to save: the mappings are written with the main file.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class BankFeedConfigurationService : IBankFeedConfigurationService, ISupportsModelPersistence
{
    private readonly IBankFeedCredentialStore credentialStore;
    private readonly IDirtyDataService dirtyDataService;
    private readonly ILogger logger;
    private readonly MapperBankFeedAccountMappingToDto mappingMapper;
    private readonly IBankFeedProvider provider;
    private ApplicationDatabase? applicationDatabase;

    public BankFeedConfigurationService(
        IBankFeedProvider provider,
        IBankFeedCredentialStore credentialStore,
        IAccountTypeRepository accountTypeRepository,
        IDirtyDataService dirtyDataService,
        ILogger logger)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        this.dirtyDataService = dirtyDataService ?? throw new ArgumentNullException(nameof(dirtyDataService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mappingMapper = new MapperBankFeedAccountMappingToDto(accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository)));
    }

    public event EventHandler? ConfigurationChanged;

    public IReadOnlyList<BankFeedCredentialField> CredentialFields => this.provider.CredentialFields;

    public bool IsBudgetFileOpen => this.applicationDatabase is not null;

    public bool IsConfigured =>
        CredentialFields.All(credentialField => HasStoredValue(credentialField.Key))
        && this.credentialStore.GetConnectionVerified(this.provider.ProviderKey) is not null
        && Mappings.Count > 0;

    public IReadOnlyList<BankFeedAccountMapping> Mappings { get; private set; } = [];

    public string ProviderDisplayName => this.provider.DisplayName;

    public string ProviderSetupInstructionsText => this.provider.SetupInstructionsText;

    public Uri? ProviderSetupInstructionsUrl => this.provider.SetupInstructionsUrl;

    public ApplicationDataType DataType => ApplicationDataType.BankFeed;

    public int LoadSequence => 50;

    public void ClearCredentials()
    {
        this.credentialStore.Clear(this.provider.ProviderKey);
        OnConfigurationChanged();
    }

    public async Task<BankFeedConnectionStatus> GetConnectionStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await this.provider.TestConnectionAsync(cancellationToken);
        }
        catch (BankFeedAuthenticationException)
        {
            NotifyCredentialsRejected();
            throw;
        }
    }

    public async Task<IReadOnlyList<BankFeedAccount>> GetFeedAccountsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await this.provider.GetAccountsAsync(cancellationToken);
        }
        catch (BankFeedAuthenticationException)
        {
            NotifyCredentialsRejected();
            throw;
        }
    }

    public bool HasStoredValue(string fieldKey)
    {
        return !string.IsNullOrWhiteSpace(this.credentialStore.Get(this.provider.ProviderKey, fieldKey));
    }

    public void NotifyCredentialsRejected()
    {
        this.credentialStore.SetConnectionVerified(this.provider.ProviderKey, null);
        OnConfigurationChanged();
    }

    public void SaveCredentials(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var unknownKeys = values.Keys.Where(key => CredentialFields.All(credentialField => credentialField.Key != key)).ToList();
        if (unknownKeys.Any())
        {
            throw new ArgumentException($"Unknown credential field(s): {string.Join(", ", unknownKeys)}", nameof(values));
        }

        this.credentialStore.Save(this.provider.ProviderKey, values);
        OnConfigurationChanged();
    }

    public void SaveMappings(IEnumerable<BankFeedAccountMapping> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        if (this.applicationDatabase is null)
        {
            throw new InvalidOperationException("A Budget Analyser file must be open to save bank feed account mappings.");
        }

        var mappingList = mappings.ToList();
        var duplicateIds = mappingList.GroupBy(m => m.ExternalAccountId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateIds.Any())
        {
            throw new ArgumentException("Each bank feed account can only be mapped once.", nameof(mappings));
        }

        Mappings = mappingList;
        this.applicationDatabase.BankFeedAccountMappings = mappingList.Select(this.mappingMapper.ToDto).ToList();
        this.dirtyDataService.NotifyOfChange(ApplicationDataType.BankFeed);
        OnConfigurationChanged();
    }

    public async Task<BankFeedConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken)
    {
        var status = await GetConnectionStatusAsync(cancellationToken);
        this.credentialStore.SetConnectionVerified(this.provider.ProviderKey, status.IsConnected ? DateTimeOffset.UtcNow : null);
        OnConfigurationChanged();
        return status;
    }

    public void Close()
    {
        this.applicationDatabase = null;
        Mappings = [];
        OnConfigurationChanged();
    }

    public Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        return LoadAsync(applicationDatabase);
    }

    public Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        this.applicationDatabase = applicationDatabase ?? throw new ArgumentNullException(nameof(applicationDatabase));
        var mappings = new List<BankFeedAccountMapping>();
        foreach (var dto in applicationDatabase.BankFeedAccountMappings)
        {
            if (this.mappingMapper.IsKnownAccount(dto))
            {
                mappings.Add(this.mappingMapper.ToModel(dto));
            }
            else
            {
                this.logger.LogWarning(_ => $"A bank feed account mapping refers to account '{dto.Account}' which does not exist. The mapping has been ignored.");
            }
        }

        Mappings = mappings;
        OnConfigurationChanged();
        return Task.CompletedTask;
    }

    public Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        // Nothing to do, the mappings are saved as part of the main Budget Analyser file.
        return Task.CompletedTask;
    }

    public void SavePreview()
    {
    }

    public bool ValidateModel(StringBuilder messages)
    {
        return true;
    }

    private void OnConfigurationChanged()
    {
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}
