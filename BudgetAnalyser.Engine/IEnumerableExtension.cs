using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine
{
    public static class IEnumerableExtension
    {
        public static bool None<T>(this IEnumerable<T> instance)
        {
            return !instance.Any();
        }
    }
}