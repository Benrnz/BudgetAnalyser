using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The merchant Akahu associated with a transaction.
/// </summary>
internal record AkahuMerchantDto([property: JsonPropertyName("_id")] string? Id, string? Name, string? Website);
