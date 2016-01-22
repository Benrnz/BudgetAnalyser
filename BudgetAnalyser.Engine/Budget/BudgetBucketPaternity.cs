using System.Diagnostics.CodeAnalysis;

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
        /// <summary>
        ///     Returns true if the two buckets are considered to be in the same family.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "I prefer instance classes over static for ease of testing and extension")]
        public bool OfSameBucketFamily(BudgetBucket bucket1, BudgetBucket bucket2)
        {
            // TODO is this the same as BucketComparer?
            if (bucket1 is SurplusBucket && bucket2 is SurplusBucket)
            {
                // All surplus subclasses are considered a subset of Surplus.
                return true;
            }

            return bucket1 == bucket2;
        }
    }
}