using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.Matching.Data;

/// <summary>
///     A Dto object to persist a matching rule.
/// </summary>
[JsonDerivedType(typeof(MatchingRuleDto), "base")]
[JsonDerivedType(typeof(SingleUseMatchingRuleDto), "singleuse")]
public record MatchingRuleDto(
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
{
    public Guid? RuleId { get; init; } = Guid.NewGuid();
}
