using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine
{
    public static class EnumerableExtension
    {
        public static bool None<T>(this IEnumerable<T> instance)
        {
            return !instance.Any();
        }

        public static double SafeAverage<T>(this IEnumerable<T> instance, Func<T, double> selector)
        {
            List<T> copy = instance.ToList();
            if (copy.None())
            {
                return 0;
            }
            return copy.Average(selector);
        }

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