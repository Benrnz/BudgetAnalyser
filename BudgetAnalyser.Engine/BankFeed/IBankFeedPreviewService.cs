using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Shows how an account's recent bank feed transactions would be imported with a given field mapping, without importing anything.
/// </summary>
public interface IBankFeedPreviewService
{
    /// <summary>
    ///     The number of days of history requested for a preview.
    /// </summary>
    const int PreviewDays = 30;

    /// <summary>
    ///     Downloads the account's transactions for the <see cref="PreviewDays" /> days up to the last business day before <paramref name="today" />, converts them using the mapping, and returns the
    ///     newest <paramref name="count" />, newest first. Nothing is added to the transactions list.
    /// </summary>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the credentials are rejected. The connection is marked as no longer verified.</exception>
    /// <exception cref="BankFeedException">Thrown when the account cannot be downloaded.</exception>
    Task<IReadOnlyList<Transaction>> PreviewAsync(BankFeedAccountMapping mapping, DateOnly today, int count, CancellationToken cancellationToken);
}
