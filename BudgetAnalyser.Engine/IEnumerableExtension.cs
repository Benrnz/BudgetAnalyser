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
    }
}