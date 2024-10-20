using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An extension class to extend <see cref="IList{T}" />.
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        ///     Will add the <paramref name="newElement" /> only if it is not null.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="instance">The target collection</param>
        /// <param name="newElement">The new element to add if its not null.</param>
        public static bool AddIfSomething<T>([NotNull] this IList<T> instance, T newElement) where T : class
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            if (newElement is not null)
            {
                instance.Add(newElement);
                return true;
            }

            return false;
        }
    }
}