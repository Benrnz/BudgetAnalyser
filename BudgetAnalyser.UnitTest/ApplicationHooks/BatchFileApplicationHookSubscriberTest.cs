using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.ApplicationHooks
{
    [TestClass]
    public class BatchFileApplicationHookSubscriberTest
    {
        private IList<Mock<IApplicationHookEventPublisher>> MockPublishers { get; set; }

        private IEnumerable<IApplicationHookEventPublisher> Publishers
        {
            get { return MockPublishers.Select(m => m.Object); }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new BatchFileApplicationHookSubscriber(null);
            Assert.Fail();
        }

        [TestMethod]
        public void FileNameShouldReturnRunFolderGivenSetterHasntBeenSet()
        {
            var subject = Arrange();
            var result = subject.PrivateFileName;

            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(result)));
        }

        [TestMethod]
        public void SubscribeShouldSubscribeAndRespondToPublisherEvent()
        {
            BatchFileApplicationHookSubscriberTestHarness subject = Arrange();
            bool eventCalled = false;
            Task internalTask = null;
            subject.PerformActionOverride = (s, e) => internalTask = Task.Factory.StartNew(() => eventCalled = true);

            subject.Subscribe(Publishers);
            RaisePublisherEvents();

            internalTask.Wait();
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscribeShouldThrowIfPublishersIsNull()
        {
            BatchFileApplicationHookSubscriberTestHarness subject = Arrange();
            subject.Subscribe(null);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MockPublishers = new List<Mock<IApplicationHookEventPublisher>>
            {
                new Mock<IApplicationHookEventPublisher>(),
            };
        }

        private BatchFileApplicationHookSubscriberTestHarness Arrange()
        {
            return new BatchFileApplicationHookSubscriberTestHarness();
        }

        private void RaisePublisherEvents()
        {
            MockPublishers.ToList().ForEach(p => p.Raise(m => m.ApplicationEvent += null, new ApplicationHookEventArgs(ApplicationHookEventType.Application, "TestOrigin", "TestSubcategory")));
        }
    }
}