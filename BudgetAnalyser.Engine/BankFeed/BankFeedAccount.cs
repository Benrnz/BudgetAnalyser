namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     An account available through a bank feed that can supply transactions.
/// </summary>
/// <param name="ExternalId">The provider's opaque identifier for the account.</param>
/// <param name="Institution">The name of the bank or institution that holds the account.</param>
/// <param name="Name">The account name or nickname.</param>
/// <param name="AccountNumber">The formatted account number, if available. For display only, never persist this.</param>
/// <param name="IsActive">True if the bank feed can currently retrieve data for this account.</param>
/// <param name="DataAsOf">When the bank feed last refreshed this account's transactions from the bank, if known.</param>
public record BankFeedAccount(string ExternalId, string Institution, string Name, string? AccountNumber, bool IsActive, DateTimeOffset? DataAsOf);
