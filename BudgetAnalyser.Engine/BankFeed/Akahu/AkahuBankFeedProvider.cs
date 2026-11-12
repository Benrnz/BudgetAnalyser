using BudgetAnalyser.Engine.BankFeed.Akahu.Data;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed.Akahu;

/// <summary>
///     Adapts the Akahu open finance API (personal app) to the vendor-neutral <see cref="IBankFeedProvider" />. This is the only place Akahu is referenced outside its own
///     DTOs and mappers; nothing outside the BudgetAnalyser.Engine.BankFeed.Akahu namespace may depend on these types.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal sealed class AkahuBankFeedProvider : IBankFeedProvider, IDisposable
{
    /// <summary>
    ///     The stable provider key. This must never change once used: stored credentials and transaction identifiers are derived from it. It is not a secret.
    /// </summary>
    internal const string Key = "rees.budgetanalyser.akahu";

    internal const string AppIdTokenField = "AppIdToken";
    internal const string UserAccessTokenField = "UserAccessToken";

    /// <summary>
    ///     A personal app can read transactions from 730 days before the user granted access.
    /// </summary>
    private const int PersonalAppHistoryDays = 730;

    private readonly MapperAkahuAccountToBankFeedAccount accountMapper = new();
    private readonly AkahuApiClient apiClient;
    private readonly IBankFeedCredentialStore credentialStore;
    private readonly MapperAkahuTransactionToBankFeedTransaction transactionMapper = new();

    public AkahuBankFeedProvider(IBankFeedCredentialStore credentialStore, ILogger logger)
        : this(credentialStore, logger, new HttpClientHandler())
    {
    }

    /// <summary>
    ///     Used by unit tests to supply a fake <see cref="HttpMessageHandler" /> so no requests leave the machine.
    /// </summary>
    internal AkahuBankFeedProvider(IBankFeedCredentialStore credentialStore, ILogger logger, HttpMessageHandler handler)
    {
        this.credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        this.apiClient = new AkahuApiClient(handler, logger ?? throw new ArgumentNullException(nameof(logger)));
    }

    public IReadOnlyList<BankFeedCredentialField> CredentialFields { get; } =
    [
        new(AppIdTokenField, "App ID Token", true),
        new(UserAccessTokenField, "User Access Token", true)
    ];

    public string DisplayName => "Akahu";

    public string ProviderKey => Key;

    public string SetupInstructionsText => "For more information and New Zealand setup instructions, see";

    public Uri SetupInstructionsUrl { get; } = new("https://akahu.com/personal-app");

    /// <summary>
    ///     Akahu refreshes from the banks periodically, so the latest day downloaded can be missing transactions that arrived after its last refresh. That day is downloaded
    ///     again next time to pick them up.
    /// </summary>
    public int SuggestedStartOverlapDays => 1;

    public void Dispose()
    {
        this.apiClient.Dispose();
    }

    public async Task<IReadOnlyList<BankFeedAccount>> GetAccountsAsync(CancellationToken cancellationToken)
    {
        var accounts = await this.apiClient.GetAccountsAsync(GetCredentials(), cancellationToken);
        return accounts
            .Where(MapperAkahuAccountToBankFeedAccount.SupportsTransactions)
            .Select(this.accountMapper.ToModel)
            .ToList();
    }

    public async Task<IReadOnlyList<BankFeedTransaction>> GetTransactionsAsync(string externalAccountId, ImportDateRange range, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalAccountId);
        if (range.Start is null)
        {
            throw new ArgumentException("A start date is required to download transactions.", nameof(range));
        }

        if (range.IsEmpty)
        {
            return [];
        }

        var credentials = GetCredentials();

        // Akahu transactions do not include the institution, it comes from the account.
        var accounts = await this.apiClient.GetAccountsAsync(credentials, cancellationToken);
        var account = accounts.FirstOrDefault(a => a.Id == externalAccountId)
                      ?? throw new BankFeedException("The account could not be found in the bank feed. It may have been disconnected.");

        var transactions = await this.apiClient.GetAllTransactionsAsync(externalAccountId, range.Start.Value, range.End, credentials, cancellationToken);
        var institution = account.Connection?.Name ?? string.Empty;
        return transactions.Select(t => this.transactionMapper.ToModel(t, institution)).ToList();
    }

    public Task<bool> RequestRefreshAsync(CancellationToken cancellationToken)
    {
        return this.apiClient.RequestRefreshAsync(GetCredentials(), cancellationToken);
    }

    public async Task<BankFeedConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken)
    {
        var me = await this.apiClient.GetMeAsync(GetCredentials(), cancellationToken);
        DateOnly? historyAvailableFrom = me.AccessGrantedAt is null ? null : AkahuDates.ToNewZealandDate(me.AccessGrantedAt.Value).AddDays(-PersonalAppHistoryDays);
        return new BankFeedConnectionStatus(true, me.Email, historyAvailableFrom);
    }

    private AkahuCredentials GetCredentials()
    {
        var appIdToken = this.credentialStore.Get(Key, AppIdTokenField);
        var userAccessToken = this.credentialStore.Get(Key, UserAccessTokenField);
        if (string.IsNullOrWhiteSpace(appIdToken) || string.IsNullOrWhiteSpace(userAccessToken))
        {
            throw new BankFeedAuthenticationException("Bank feed credentials have not been saved. Enter them on the Settings tab.");
        }

        return new AkahuCredentials(appIdToken, userAccessToken);
    }
}
