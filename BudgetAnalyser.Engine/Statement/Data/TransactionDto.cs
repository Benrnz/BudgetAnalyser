namespace BudgetAnalyser.Engine.Statement.Data;

/// <summary>
///     A Dto to persist a single transaction from a statement.
/// </summary>
public record TransactionDto(
    string Account,
    decimal Amount,
    DateOnly Date,
    Guid Id,
    string TransactionType,
    string? BudgetBucketCode = null,
    string? Description = null,
    string? Reference1 = null,
    string? Reference2 = null,
    string? Reference3 = null);
