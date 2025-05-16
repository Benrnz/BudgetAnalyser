namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a collection of budgets.
/// </summary>
public record BudgetCollectionDto(BudgetBucketDto[] Buckets, BudgetModelDto[] Budgets, string StorageKey);
