namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     Currency conversion details for a transaction made in another currency.
/// </summary>
internal record AkahuConversionDto(decimal Amount, string? Currency, decimal Rate);
