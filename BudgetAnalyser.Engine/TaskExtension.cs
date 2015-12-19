using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An extension to the <see cref="Task" /> and <see cref="IEnumerable{T}" /> types.
    /// </summary>
    public static class TaskExtension
    {
        /// <summary>
        ///     Returns a task that will complete when all tasks in the collection are complete.
        /// </summary>
        public static Task ContinueWhenAllTasksComplete([NotNull] this IEnumerable<Task> instance)
        {
            return Task.WhenAll(instance);
        }
    }
}