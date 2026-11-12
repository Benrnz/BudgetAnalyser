using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     A source of bank transactions that are downloaded through an online bank feed, rather than imported from a bank extract file.
///     Implementations adapt a specific vendor's API to this vendor-neutral contract. No vendor specific types are exposed through this interface.
/// </summary>
public interface IBankFeedProvider
{
    /// <summary>
    ///     Gets the credential fields this provider needs the user to supply. These are used to build the settings UI. All values are stored in an <see cref="IBankFeedCredentialStore" />.
    /// </summary>
    IReadOnlyList<BankFeedCredentialField> CredentialFields { get; }

    /// <summary>
    ///     Gets the display name of the provider, for display purposes only.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///     Gets an opaque and stable key that identifies this provider. It is used to namespace stored credentials and to derive stable transaction identifiers, so it must never change once used.
    /// </summary>
    string ProviderKey { get; }

    /// <summary>
    ///     Gets a short sentence that introduces <see cref="SetupInstructionsUrl" />, for example "For setup instructions, see". Empty if the provider has no setup instructions.
    /// </summary>
    string SetupInstructionsText { get; }

    /// <summary>
    ///     Gets the web address of the provider's setup instructions, or null if it has none. Shown to the user as a link on the Settings tab.
    /// </summary>
    Uri? SetupInstructionsUrl { get; }

    /// <summary>
    ///     Gets the accounts available through the bank feed that can supply transactions.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<IReadOnlyList<BankFeedAccount>> GetAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all settled transactions for one account for the given inclusive date range. The implementation retrieves every page of results before returning;
    ///     if any part of the download fails an exception is thrown and no partial results are returned.
    /// </summary>
    /// <param name="externalAccountId">The provider's identifier for the account, from <see cref="BankFeedAccount.ExternalId" />.</param>
    /// <param name="range">The inclusive range of dates to retrieve. <see cref="ImportDateRange.Start" /> must not be null.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached, returns an error, or the account cannot be found.</exception>
    Task<IReadOnlyList<BankFeedTransaction>> GetTransactionsAsync(string externalAccountId, ImportDateRange range, CancellationToken cancellationToken);

    /// <summary>
    ///     Asks the bank feed to refresh its data for every account from the banks. The refresh happens afterwards and can take a while; this returns as soon as the request is
    ///     accepted. <see cref="BankFeedAccount.DataAsOf" /> moves forward once an account has been refreshed.
    /// </summary>
    /// <returns>True if the request was accepted. False if the bank feed declined it because a refresh was requested only a few minutes ago.</returns>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<bool> RequestRefreshAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Tests that the stored credentials are accepted by the bank feed.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the stored credentials are missing or rejected.</exception>
    /// <exception cref="BankFeedException">Thrown when the bank feed cannot be reached or returns an error.</exception>
    Task<BankFeedConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken);
}
