using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public static class TaskExtension
    {
        public static Task ContinueWhenAllTasksComplete([NotNull] this IEnumerable<Task> instance)
        {
            return Task.WhenAll(instance);
        }
    }
}
