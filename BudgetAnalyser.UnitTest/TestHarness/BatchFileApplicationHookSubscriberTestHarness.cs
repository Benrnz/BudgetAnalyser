using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BatchFileApplicationHookSubscriberTestHarness : BatchFileApplicationHookSubscriber
    {
        public BatchFileApplicationHookSubscriberTestHarness() : base(new FakeLogger())
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
