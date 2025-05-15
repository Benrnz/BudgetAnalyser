namespace BudgetAnalyser.Engine.Matching.Data;

/// <summary>
///     A Dto for a single use matching rule. This is a rule that will be deleted after it has been used to match a transaction.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Matching.Data.MatchingRuleDto" />
public record SingleUseMatchingRuleDto(
    decimal? Amount,
    bool And,
    string BucketCode,
    DateTime? Created,
    string? Description,
    DateTime? LastMatch,
    int MatchCount,
    string? Reference1,
    string? Reference2,
    string? Reference3,
    string? TransactionType)
    : MatchingRuleDto(Amount, And, BucketCode, Created, Description, LastMatch, MatchCount, Reference1, Reference2, Reference3, TransactionType);
