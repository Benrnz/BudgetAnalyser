namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a collection of budgets.
/// </summary>
public record BudgetCollectionDto
{
    /// <summary>
    ///     Gets or sets the buckets included in the budget collection.
    /// </summary>
    public List<BudgetBucketDto> Buckets { get; init; } = new();

    /// <summary>
    ///     Gets or sets the budgets included in the budget collection.
    /// </summary>
    public List<BudgetModelDto> Budgets { get; init; } = new();

    /// <summary>
    ///     Gets or sets the storage key.
    /// </summary>
    public string StorageKey { get; init; } = string.Empty;
}
