using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An extension class for <see cref="IEnumerable{T}" />
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        ///     Returns true if there are no elements in the collection. This is the logical opporsite to Any()
        /// </summary>
        public static bool None<T>(this IEnumerable<T> instance)
        {
            if (instance == null) return false;
            return !instance.Any();
        }

        /// <summary>
        ///     Calculates an average and is tolerant of empty collections.
        /// </summary>
        public static double SafeAverage<T>(this IEnumerable<T> instance, Func<T, double> selector)
        {
            List<T> copy = instance.ToList();
            if (copy.None())
            {
                return 0;
            }
            return copy.Average(selector);
        }

        /// <summary>
        ///     Calculates an average and is tolerant of empty collections.
        /// </summary>
        public static decimal SafeAverage<T>(this IEnumerable<T> instance, Func<T, decimal> selector)
        {
            List<T> copy = instance.ToList();
            if (copy.None())
            {
                return 0;
            }
            return copy.Average(selector);
        }
    }
}