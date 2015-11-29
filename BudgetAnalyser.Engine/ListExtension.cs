using System.Collections.Generic;

namespace BudgetAnalyser.Engine
{
    public static class ListExtension
    {
        public static void AddIfSomething<T>(this IList<T> instance, T newElement) where T : class 
        {
            if (newElement != null)
            {
                instance.Add(newElement);
            }
        }
    }
}
