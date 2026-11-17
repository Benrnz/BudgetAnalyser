namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     An Akahu account balance.
/// </summary>
internal record AkahuBalanceDto(string? Currency, decimal Current, decimal? Available, decimal? Limit, bool? Overdrawn);
