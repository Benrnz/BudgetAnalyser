namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a single Budget Bucket
/// </summary>
public class BudgetBucketDto
{
    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="BudgetBucketDto" /> is active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if active; otherwise, <c>false</c>.
    /// </value>
    public bool Active { get; init; }

    /// <summary>
    ///     Gets or sets the bucket code. This uniquely identifies the bucket.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    ///     Gets or sets the bucket description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets or sets the simple persistence type representing the kind of bucket.
    /// </summary>
    public virtual BucketDtoType Type { get; init; }
}
