using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     An equality comparer that treats any subclass of <see cref="SurplusBucket" /> as a <see cref="SurplusBucket" />.
    ///     This is ideal for joining two collections using Linq on the <see cref="BudgetBucket" /> property in each
    ///     collection.
    /// </summary>
    public class SurplusAgnosticBucketComparer : IEqualityComparer<BudgetBucket>
    {
        public bool Equals(BudgetBucket x, BudgetBucket y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            if (x is SurplusBucket && y is SurplusBucket)
            {
                return true;
            }

            return ReferenceEquals(x, y);
        }

        /// <summary>
        /// Linq Join uses this method.
        /// </summary>
        public int GetHashCode(BudgetBucket obj)
        {
            if (obj == null)
            {
                return 0;
            }
            if (obj is SurplusBucket)
            {
                return 1; // All surplus buckets are treated the same.
            }

            return obj.GetHashCode();
        }
    }
}