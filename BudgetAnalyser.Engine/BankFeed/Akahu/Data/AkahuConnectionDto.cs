using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The institution an Akahu account is connected to.
/// </summary>
/// <param name="Id">The Akahu connection identifier.</param>
/// <param name="Name">The institution's display name, for example the bank's name.</param>
/// <param name="Logo">A URL of the institution's logo.</param>
/// <param name="ConnectionType">classic or official.</param>
internal record AkahuConnectionDto([property: JsonPropertyName("_id")] string Id, string? Name, string? Logo, string? ConnectionType);
