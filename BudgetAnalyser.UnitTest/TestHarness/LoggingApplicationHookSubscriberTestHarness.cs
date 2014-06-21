using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class LoggingApplicationHookSubscriberTestHarness : LoggingApplicationHookSubscriber
    {
        public LoggingApplicationHookSubscriberTestHarness([NotNull] ILogger logger)
            : base(logger)
        {
        }

        public Action<object, ApplicationHookEventArgs> PerformActionOverride { get; set; }

        protected override void PerformAction(object sender, ApplicationHookEventArgs args)
        {
            if (PerformActionOverride == null)
            {
                base.PerformAction(sender, args);
                return;
            }

            PerformActionOverride(sender, args);
        }
    }
}
