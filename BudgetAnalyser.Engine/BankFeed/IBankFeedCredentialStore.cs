namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Stores bank feed credentials securely for the current user, outside of the repository and outside of the budget data files.
///     Values are grouped by <see cref="IBankFeedProvider.ProviderKey" />. Implementations must never log credential values.
/// </summary>
public interface IBankFeedCredentialStore
{
    /// <summary>
    ///     Removes all stored values, and the connection verified time, for the given provider.
    /// </summary>
    void Clear(string providerKey);

    /// <summary>
    ///     Gets a stored credential value, or null if it has not been stored.
    /// </summary>
    string? Get(string providerKey, string fieldKey);

    /// <summary>
    ///     Gets when a connection test using the currently stored credentials last succeeded, or null if it has not succeeded since the credentials were saved.
    /// </summary>
    DateTimeOffset? GetConnectionVerified(string providerKey);

    /// <summary>
    ///     Replaces all stored values for the given provider. This also clears the connection verified time, because the new values have not been tested.
    /// </summary>
    void Save(string providerKey, IReadOnlyDictionary<string, string> values);

    /// <summary>
    ///     Records when a connection test using the currently stored credentials succeeded. Pass null to clear it, for example when the credentials have been rejected.
    /// </summary>
    void SetConnectionVerified(string providerKey, DateTimeOffset? verifiedUtc);
}
