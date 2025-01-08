using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

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
    [UsedImplicitly]
    public bool Active { get; set; }

    /// <summary>
    ///     Gets or sets the bucket code. This uniquely identifies the bucket.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the bucket description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the simple persistence type representing the kind of bucket.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Permissable in this case as it is linked to the type.")]
    public virtual BucketDtoType Type { get; set; }
}
