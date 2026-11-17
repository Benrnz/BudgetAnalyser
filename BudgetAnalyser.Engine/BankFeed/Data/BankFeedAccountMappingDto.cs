using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed.Data;

/// <summary>
///     The persisted form of a <see cref="BankFeedAccountMapping" />, stored inside the main Budget Analyser file. That file is not encrypted, so this must never hold anything
///     sensitive: no credentials, account numbers, names, balances or institution names. The external account identifier is an opaque provider identifier that is useless without
///     the credentials, which are stored separately.
/// </summary>
/// <param name="ExternalAccountId">The provider's opaque identifier for the account.</param>
/// <param name="Account">The Budget Analyser account key, for example "CHEQUE".</param>
/// <param name="FieldMapping">The field mapping. Optional, a missing mapping loads as <see cref="BankFeedFieldMapping.Default" />.</param>
/// <param name="TypeNames">The transaction type names table. Optional, missing means no renames.</param>
public record BankFeedAccountMappingDto(
    string ExternalAccountId,
    string Account,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BankFeedFieldMappingDto? FieldMapping = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Dictionary<string, string>? TypeNames = null);
