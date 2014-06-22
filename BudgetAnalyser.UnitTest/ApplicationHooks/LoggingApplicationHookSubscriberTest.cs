using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.ApplicationHooks
{
    [TestClass]
    public class LoggingApplicationHookSubscriberTest
    {
        private Mock<ILogger> MockLogger { get; set; }
        private IList<Mock<IApplicationHookEventPublisher>> MockPublishers { get; set; }

        private IEnumerable<IApplicationHookEventPublisher> Publishers
        {
            get { return MockPublishers.Select(m => m.Object); }
        }

        [TestMethod]
        public void ShouldRespondToPublisherEventByCallerLogger()
        {
            LoggingApplicationHookSubscriberTestHarness subject = Arrange();
            subject.Subscribe(Publishers);

            RaisePublisherEvents();

            MockLogger.Verify(m => m.LogInfo(It.IsAny<Func<string>>()));
        }

        [TestMethod]
        public void SubscribeShouldSubscribeAndRespondToPublisherEvent()
        {
            LoggingApplicationHookSubscriberTestHarness subject = Arrange();
            bool eventCalled = false;
            subject.PerformActionOverride = (s, e) => eventCalled = true;

            subject.Subscribe(Publishers);
            RaisePublisherEvents();

            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscribeShouldThrowIfPublishersIsNull()
        {
            LoggingApplicationHookSubscriberTestHarness subject = Arrange();
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

            MockLogger = new Mock<ILogger>();
        }

        private LoggingApplicationHookSubscriberTestHarness Arrange()
        {
            return new LoggingApplicationHookSubscriberTestHarness(MockLogger.Object);
        }

        private void RaisePublisherEvents()
        {
            MockPublishers.ToList().ForEach(p => p.Raise(m => m.ApplicationEvent += null, new ApplicationHookEventArgs(ApplicationHookEventType.Application, "TestOrigin", "TestSubcategory")));
        }
    }
}