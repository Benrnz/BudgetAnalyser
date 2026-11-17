namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     The result of successfully testing the connection to a bank feed.
/// </summary>
/// <param name="IsConnected">True if the bank feed accepted the stored credentials.</param>
/// <param name="ConnectedAs">A description of the bank feed user the credentials belong to, if the provider supplies one.</param>
/// <param name="HistoryAvailableFrom">The earliest date transactions can be retrieved from, if the provider supplies one.</param>
public record BankFeedConnectionStatus(bool IsConnected, string? ConnectedAs, DateOnly? HistoryAvailableFrom);
