using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BudgetAnalyser.UnitTest.TestHarness;
using log4net;
using log4net.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BudgetAnalyserLog4NetLoggerTest
    {
        private Mock<ILog> MockLogger { get; set; }
        private LoggerTestHarness Subject { get; set; }

        public LoggerTestHarness Arrange()
        {
            return new LoggerTestHarness(MockLogger);
        }

        [TestMethod]
        public void CtorShouldCallConfigure()
        {
            Assert.IsTrue(Subject.ConfigWasCalled);
        }

        [TestMethod]
        public void CtorShouldInitialiseInternalLogger()
        {
            Assert.IsTrue(Subject.HasInternalLogger);
        }

        [TestMethod]
        public void FormatShouldFormatAValidString()
        {
            Assert.AreEqual("Foo Bar. Poo Far", Subject.Format("Foo {0}. {1} Far", "Bar", "Poo"));
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void FormatShouldThrowIfDisposed()
        {
            Subject.Dispose();
            Subject.Format("Foo", "Bar");
            Assert.Fail();
        }

        [TestMethod]
        public void LogAlwaysShouldBeThreadSafe()
        {
            var bag = new ConcurrentBag<string>();
            MockLogger.Setup(m => m.Error(It.IsAny<object>())).Callback<object>(msg => bag.Add(msg.ToString()));
            MockLogger.Setup(m => m.Info(It.IsAny<object>())).Callback<object>(msg => bag.Add(msg.ToString()));
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(true);

            var threads = new List<Thread>();
            for (var threadNumber = 0; threadNumber < 20; threadNumber++)
            {
                if (threadNumber % 2 == 0)
                {
                    threads.Add(
                        new Thread(
                            () =>
                            {
                                for (var i = 0; i < 10; i++)
                                {
                                    Subject.LogError(_ => "Foo");
                                }
                            }));
                }
                else
                {
                    threads.Add(
                        new Thread(
                            () =>
                            {
                                for (var i = 0; i < 10; i++)
                                {
                                    Subject.LogAlways(_ => "Always");
                                }
                            }));
                }
            }

            threads.ForEach(t => t.Start());
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(200, bag.Count);
            Assert.AreEqual(100, bag.Count(msg => msg == "Foo"));
            Assert.AreEqual(100, bag.Count(msg => msg == "Always"));
        }

        [TestMethod]
        public void LogAlwaysShouldLogWhenErrorLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(false);
            Subject.LogAlways(_ => "Foo");
            MockLogger.Verify(m => m.Info(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogAlwaysShouldLogWhenInfoLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsInfoEnabled).Returns(false);
            Subject.LogAlways(_ => "Foo");
            MockLogger.Verify(m => m.Info(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogAlwaysShouldLogWhenWarningLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsWarnEnabled).Returns(false);
            Subject.LogAlways(_ => "Foo");
            MockLogger.Verify(m => m.Info(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogAlwaysShouldNotChangeLogLevel()
        {
            Level level = Subject.LoggingLevel;
            Subject.LogAlways(_ => "The quick brown fox jumped over the lazy dog.");
            Assert.AreEqual(level, Subject.LoggingLevel);
        }

        [TestMethod]
        public void LogErrorShouldLogWhenErrorLoggingIsEnabled()
        {
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(true);
            Subject.LogError(_ => "Foo");
            MockLogger.Verify(m => m.Error(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogErrorShouldNotLogWhenErrorLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(false);
            MockLogger.Setup(m => m.Error(It.IsAny<string>())).Throws(new Exception());
            Subject.LogError(_ => "Foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogErrorShouldThrowGivenNullBuilder()
        {
            Subject.LogError(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void LogErrorShouldThrowIfDisposed()
        {
            Subject.Dispose();
            Subject.LogError(_ => "Foo");
            Assert.Fail();
        }

        [TestMethod]
        public void LogErrorWithExceptionShouldLogWhenErrorLoggingIsEnabled()
        {
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(true);
            Subject.LogError(new Exception(), _ => "Foo");
            MockLogger.Verify(m => m.Error(It.IsAny<string>(), It.IsAny<Exception>()));
        }

        [TestMethod]
        public void LogErrorWithExceptionShouldNotLogWhenErrorLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsErrorEnabled).Returns(false);
            MockLogger.Setup(m => m.Error(It.IsAny<string>())).Throws(new Exception());
            Subject.LogError(new Exception(), _ => "Foo");
        }

        [TestMethod]
        public void LogErrorWithExceptionShouldNotThrowGivenNullException()
        {
            Subject.LogError(null, _ => "Foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogErrorWithExceptionShouldThrowGivenNullBuilder()
        {
            Subject.LogError(new Exception(), null);
            Assert.Fail();
        }

        [TestMethod]
        public void LogInfoShouldLogWhenErrorLoggingIsEnabled()
        {
            MockLogger.Setup(m => m.IsInfoEnabled).Returns(true);
            Subject.LogInfo(_ => "Foo");
            MockLogger.Verify(m => m.Info(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogInfoShouldNotLogWhenErrorLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsInfoEnabled).Returns(false);
            MockLogger.Setup(m => m.Info(It.IsAny<string>())).Throws(new Exception());
            Subject.LogInfo(_ => "Foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogInfoShouldThrowGivenNullBuilder()
        {
            Subject.LogInfo(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void LogInfoShouldThrowIfDisposed()
        {
            Subject.Dispose();
            Subject.LogInfo(_ => "Foo");
            Assert.Fail();
        }

        [TestMethod]
        public void LogWarningShouldLogWhenErrorLoggingIsEnabled()
        {
            MockLogger.Setup(m => m.IsWarnEnabled).Returns(true);
            Subject.LogWarning(_ => "Foo");
            MockLogger.Verify(m => m.Warn(It.IsAny<string>()));
        }

        [TestMethod]
        public void LogWarningShouldNotLogWhenErrorLoggingIsDisabled()
        {
            MockLogger.Setup(m => m.IsWarnEnabled).Returns(false);
            MockLogger.Setup(m => m.Warn(It.IsAny<string>())).Throws(new Exception());
            Subject.LogWarning(_ => "Foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogWarningShouldThrowGivenNullBuilder()
        {
            Subject.LogWarning(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void LogWarningShouldThrowIfDisposed()
        {
            Subject.Dispose();
            Subject.LogWarning(_ => "Foo");
            Assert.Fail();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Subject.Dispose();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MockLogger = new Mock<ILog>();
            Subject = Arrange();
        }
    }
}