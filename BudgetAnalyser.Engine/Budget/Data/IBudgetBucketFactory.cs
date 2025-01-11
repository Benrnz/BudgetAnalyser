namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A factory to build to and from persistence types for <see cref="BudgetBucket" />.
/// </summary>
public interface IBudgetBucketFactory
{
    /// <summary>
    ///     Builds a <see cref="BudgetBucketDto" /> based on the model passed in.
    /// </summary>
    BudgetBucketDto BuildDto(BudgetBucket bucket);

    /// <summary>
    ///     Builds a <see cref="BudgetBucket" /> based on the <see cref="BucketDtoType" /> for persistence purposes.
    /// </summary>
    BudgetBucket BuildModel(BudgetBucketDto dto);

    /// <summary>
    ///     Serialises the <see cref="BudgetBucket" /> to a simple persistence type.
    /// </summary>
    BucketDtoType SerialiseType(BudgetBucket bucket);
}
