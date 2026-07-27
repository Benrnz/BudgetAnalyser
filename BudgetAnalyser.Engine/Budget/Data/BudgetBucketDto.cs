namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a single Budget Bucket
/// </summary>
public record BudgetBucketDto(bool Active, string Code, string Description, BucketDtoType Type);
