using BudgetAnalyser.Engine;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BatchFileApplicationHookSubscriberTestHarness : BatchFileApplicationHookSubscriber
    {
        public BatchFileApplicationHookSubscriberTestHarness()
            : base(new FakeLogger())
        {
        }

        public string PrivateFileName
        {
            get
            {
                return (string)PrivateAccessor.GetProperty<BatchFileApplicationHookSubscriber>(this, "FileName");
            }
        }
    }
}
