namespace BudgetAnalyser.Engine.Budget.Data
{
    /// <summary>
    ///     A factory to build to and from persistence types for <see cref="BudgetBucket" />.
    /// </summary>
    public interface IBudgetBucketFactory
    {
        /// <summary>
        ///     Builds a <see cref="BudgetBucket" /> based on the <see cref="BucketDtoType" /> for persistence purposes.
        /// </summary>
        BudgetBucket Build(BucketDtoType type);

        /// <summary>
        ///     Serialises the <see cref="BudgetBucket" /> to a simple persistence type.
        /// </summary>
        BucketDtoType SerialiseType(BudgetBucket bucket);
    }
}