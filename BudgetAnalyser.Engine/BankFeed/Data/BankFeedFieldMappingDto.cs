namespace BudgetAnalyser.Engine.BankFeed.Data;

/// <summary>
///     The persisted form of the source field lists in a <see cref="BankFeedFieldMapping" />. Each list is the source field, then the optional fallback. A missing list loads as empty.
/// </summary>
public record BankFeedFieldMappingDto(
    List<BankFeedSourceField>? Description = null,
    List<BankFeedSourceField>? Reference1 = null,
    List<BankFeedSourceField>? Reference2 = null,
    List<BankFeedSourceField>? Reference3 = null,
    List<BankFeedSourceField>? TransactionType = null);
