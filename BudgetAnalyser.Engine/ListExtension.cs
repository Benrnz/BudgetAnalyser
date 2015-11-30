using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public static class ListExtension
    {
        public static void AddIfSomething<T>([NotNull] this IList<T> instance, T newElement) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (newElement != null)
            {
                instance.Add(newElement);
            }
        }
    }
}
