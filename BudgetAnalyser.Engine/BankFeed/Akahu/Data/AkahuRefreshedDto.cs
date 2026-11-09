namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     When Akahu last refreshed parts of an account's data from the bank. Each timestamp is optional.
/// </summary>
internal record AkahuRefreshedDto(DateTimeOffset? Balance, DateTimeOffset? Meta, DateTimeOffset? Transactions);
