namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object for persisting a Fixed Project Budget Bucket.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.Data.BudgetBucketDto" />
public record FixedBudgetBucketDto(
    bool Active,
    string Code,
    string Description,
    DateTime Created,
    decimal FixedBudgetAmount) : BudgetBucketDto(Active, Code, Description, BucketDtoType.FixedBudgetProject);
