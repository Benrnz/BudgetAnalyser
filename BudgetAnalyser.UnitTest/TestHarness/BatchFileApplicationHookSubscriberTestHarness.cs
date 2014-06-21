using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BatchFileApplicationHookSubscriberTestHarness : BatchFileApplicationHookSubscriber
    {
        public BatchFileApplicationHookSubscriberTestHarness([NotNull] ILogger logger) : base(logger)
        {
        }

        public Func<object, ApplicationHookEventArgs, Task> PerformActionOverride { get; set; }

        protected override Task PerformAction(object sender, ApplicationHookEventArgs args)
        {
            if (PerformActionOverride == null)
            {
                return base.PerformAction(sender, args);
            }

            return PerformActionOverride(sender, args);
        }
    }
}
