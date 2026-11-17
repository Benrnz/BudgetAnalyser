using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     A settled Akahu transaction. Only the fields Budget Analyser uses are declared; other fields in the JSON are ignored.
/// </summary>
/// <param name="Id">The Akahu transaction identifier, prefixed with "trans_". Stable between queries for the vast majority of transactions.</param>
/// <param name="Account">The Akahu account identifier the transaction belongs to.</param>
/// <param name="Connection">The Akahu connection identifier.</param>
/// <param name="Migrated">The previous transaction identifier if the account was migrated to an official open banking connection.</param>
/// <param name="CreatedAt">When Akahu first saw the transaction. Unrelated to the transaction date.</param>
/// <param name="UpdatedAt">When Akahu last updated the transaction.</param>
/// <param name="Date">When the transaction occurred, in UTC. Often only accurate to the day, so it must be converted to New Zealand time before the date part is used.</param>
/// <param name="Description">The bank's description, with minor whitespace clean up by Akahu.</param>
/// <param name="Amount">The amount. Negative values are money leaving the account.</param>
/// <param name="Balance">The account balance after the transaction, if available.</param>
/// <param name="Type">The transaction type, for example EFTPOS or STANDING ORDER. Kept as a string because Akahu may add values.</param>
/// <param name="Merchant">The merchant, if Akahu recognised it.</param>
/// <param name="Meta">Other optional data extracted from the transaction.</param>
internal record AkahuTransactionDto(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("_account")] string Account,
    [property: JsonPropertyName("_connection")] string? Connection,
    [property: JsonPropertyName("_migrated")] string? Migrated,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset Date,
    string? Description,
    decimal Amount,
    decimal? Balance,
    string? Type,
    AkahuMerchantDto? Merchant,
    AkahuTransactionMetaDto? Meta);
