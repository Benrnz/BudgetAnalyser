namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A comparison class used to determine if a bucket is considered in the same "family" as another.
    ///     For example: Any derivative of <see cref="SurplusBucket" /> is considered a subset of Surplus, so
    ///     it is considered to be in the same family. Given any <see cref="FixedBudgetProjectBucket" /> and
    ///     and any other <see cref="SurplusBucket" /> or <see cref="FixedBudgetProjectBucket" /> the
    ///     <see cref="OfSameBucketFamily" /> will return true.
    /// </summary>
    public class BudgetBucketPaternity
    {
        public bool OfSameBucketFamily(BudgetBucket bucket1, BudgetBucket bucket2)
        {
            if (bucket1 is SurplusBucket && bucket2 is SurplusBucket)
            {
                // All surplus subclasses are considered a subset of Surplus.
                return true;
            }

            return bucket1 == bucket2;
        }
    }
}