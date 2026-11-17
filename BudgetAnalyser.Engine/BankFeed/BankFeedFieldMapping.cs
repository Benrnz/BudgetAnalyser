namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Describes, for one account, which <see cref="BankFeedTransaction" /> fields fill each Budget Analyser transaction field. Each list is tried in order and the first
///     field with a value is used, so the second entry is a fallback. An empty list leaves the target field empty. This is configured by the user on the Settings tab so
///     that existing matching rules keep matching.
/// </summary>
/// <param name="Description">The sources for the transaction description.</param>
/// <param name="Reference1">The sources for reference 1.</param>
/// <param name="Reference2">The sources for reference 2.</param>
/// <param name="Reference3">The sources for reference 3.</param>
/// <param name="TransactionType">The sources for the transaction type, before <paramref name="TypeNames" /> is applied.</param>
/// <param name="TypeNames">Renames transaction types, for example "EFTPOS" to "EFTPOS TRANSACTION". Types not listed are used unchanged.</param>
public record BankFeedFieldMapping(
    IReadOnlyList<BankFeedSourceField> Description,
    IReadOnlyList<BankFeedSourceField> Reference1,
    IReadOnlyList<BankFeedSourceField> Reference2,
    IReadOnlyList<BankFeedSourceField> Reference3,
    IReadOnlyList<BankFeedSourceField> TransactionType,
    IReadOnlyDictionary<string, string> TypeNames)
{
    /// <summary>
    ///     The neutral default used until a preset or the user says otherwise: the description from the description, the type from the type, and no references.
    /// </summary>
    public static BankFeedFieldMapping Default { get; } = new(
        [BankFeedSourceField.Description],
        [],
        [],
        [],
        [BankFeedSourceField.Type],
        new Dictionary<string, string>());
}
