using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     An account connected to Akahu. Only the fields Budget Analyser uses are declared; other fields in the JSON are ignored.
/// </summary>
/// <param name="Id">The Akahu account identifier, prefixed with "acc_".</param>
/// <param name="Connection">The institution the account is held at.</param>
/// <param name="Name">The account name or nickname.</param>
/// <param name="Status">ACTIVE or INACTIVE. Kept as a string because Akahu may add values.</param>
/// <param name="Type">The account type, for example CHECKING or CREDITCARD. Kept as a string because Akahu may add values.</param>
/// <param name="Attributes">What the account can do. Transactions can only be retrieved if this contains TRANSACTIONS.</param>
/// <param name="FormattedAccount">The formatted account number, if the account has one.</param>
/// <param name="Balance">The account balance, if available.</param>
/// <param name="Refreshed">When Akahu last refreshed parts of the account data from the bank.</param>
internal record AkahuAccountDto(
    [property: JsonPropertyName("_id")] string Id,
    AkahuConnectionDto? Connection,
    string? Name,
    string? Status,
    string? Type,
    string[]? Attributes,
    string? FormattedAccount,
    AkahuBalanceDto? Balance,
    AkahuRefreshedDto? Refreshed);
