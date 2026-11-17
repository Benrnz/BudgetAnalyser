using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The Akahu user the credentials belong to.
/// </summary>
/// <param name="Id">The Akahu user identifier.</param>
/// <param name="AccessGrantedAt">When the user granted the app access. Transactions are available from 730 days before this for a personal app.</param>
/// <param name="Email">The user's email address.</param>
internal record AkahuMeDto([property: JsonPropertyName("_id")] string Id, DateTimeOffset? AccessGrantedAt, string? Email);
