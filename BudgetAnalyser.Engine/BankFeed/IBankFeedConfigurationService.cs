namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Manages the bank feed settings: the credentials (stored securely for the current Windows user) and the account mappings (stored in the open Budget Analyser file).
///     Also decides whether the bank feed is fully set up and ready to download.
/// </summary>
public interface IBankFeedConfigurationService
{
    /// <summary>
    ///     Occurs when anything that affects <see cref="IsConfigured" /> or <see cref="Mappings" /> changes, including a Budget Analyser file being opened or closed.
    /// </summary>
    event EventHandler? ConfigurationChanged;

    /// <summary>
    ///     Gets the credential fields the provider needs, to build the settings UI.
    /// </summary>
    IReadOnlyList<BankFeedCredentialField> CredentialFields { get; }

    /// <summary>
    ///     Gets a value indicating whether a Budget Analyser file is open. Account mappings can only be changed while one is open.
    /// </summary>
    bool IsBudgetFileOpen { get; }

    /// <summary>
    ///     Gets a value indicating whether the bank feed is ready to download: every credential field has a stored value, a connection test has succeeded since they were saved,
    ///     and the open Budget Analyser file maps at least one bank feed account.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    ///     Gets the account mappings stored in the open Budget Analyser file. Empty when no file is open.
    /// </summary>
    IReadOnlyList<BankFeedAccountMapping> Mappings { get; }

    /// <summary>
    ///     Gets the provider's display name, for display purposes only.
    /// </summary>
    string ProviderDisplayName { get; }

    /// <summary>
    ///     Gets a short sentence that introduces <see cref="ProviderSetupInstructionsUrl" />. Empty if the provider has no setup instructions.
    /// </summary>
    string ProviderSetupInstructionsText { get; }

    /// <summary>
    ///     Gets the web address of the provider's setup instructions, or null if it has none.
    /// </summary>
    Uri? ProviderSetupInstructionsUrl { get; }

    /// <summary>
    ///     Removes the stored credentials.
    /// </summary>
    void ClearCredentials();

    /// <summary>
    ///     Gets the current connection status from the bank feed without recording anything, for example to find how much history is available. Unlike
    ///     <see cref="TestConnectionAsync" /> a success does not change the stored connection verified time and does not raise <see cref="ConfigurationChanged" />.
    ///     If the credentials are rejected the connection is marked as no longer verified.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<BankFeedConnectionStatus> GetConnectionStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the accounts available in the bank feed. If the credentials are rejected the connection is marked as no longer verified.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<IReadOnlyList<BankFeedAccount>> GetFeedAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Returns true if a value has been stored for the given credential field. The value itself is never returned.
    /// </summary>
    bool HasStoredValue(string fieldKey);

    /// <summary>
    ///     Records that the bank feed rejected the stored credentials, so the connection is no longer verified and <see cref="IsConfigured" /> becomes false.
    /// </summary>
    void NotifyCredentialsRejected();

    /// <summary>
    ///     Replaces the stored credentials. The connection must be tested again afterwards.
    /// </summary>
    void SaveCredentials(IReadOnlyDictionary<string, string> values);

    /// <summary>
    ///     Replaces the account mappings in the open Budget Analyser file and marks it as needing to be saved.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no Budget Analyser file is open.</exception>
    void SaveMappings(IEnumerable<BankFeedAccountMapping> mappings);

    /// <summary>
    ///     Tests the stored credentials. On success the connection is marked as verified; if the credentials are rejected it is marked as not verified.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<BankFeedConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken);
}
