namespace BudgetAnalyser.Engine.UnitTest
{
    [TestClass]
    public class DebugLoggerTest
    {
        private readonly ILogger debugLogger = new DebugLogger(true);

        [TestMethod]
        public void NoLoggingShouldOccurWhenDebuggerNotAttached()
        {
            var logger = new DebugLogger(false);
            var didLog = false;
            logger.LogError(l => (didLog = true).ToString());
            logger.LogWarning(l => (didLog = true).ToString());
            logger.LogInfo(l => (didLog = true).ToString());
            logger.LogAlways(l => (didLog = true).ToString());
            Assert.IsFalse(didLog);
        }

        [TestMethod]
        public void ShouldInitialiseToAlwaysLog()
        {
            Assert.AreEqual(LogLevel.Always, this.debugLogger.LogLevelFilter);
        }

        [TestMethod]
        public void ShouldRememberLogFilterChanges()
        {
            this.debugLogger.LogLevelFilter = LogLevel.Error;
            Assert.AreEqual(LogLevel.Error, this.debugLogger.LogLevelFilter);
        }

        [TestMethod]
        [DataRow(LogLevel.Error, true)]
        [DataRow(LogLevel.Warn, true)]
        [DataRow(LogLevel.Info, true)]
        [DataRow(LogLevel.Always, true)]
        public void ShouldLogExceptionWhenFilteredToError(LogLevel filter, bool expected)
        {
            var didLog = false;
            this.debugLogger.LogLevelFilter = filter;
            this.debugLogger.LogError(new Exception("Test Exception"), l => (didLog = true).ToString());
            Assert.AreEqual(expected, didLog);
        }

        [TestMethod]
        [DataRow(LogLevel.Error, true)]
        [DataRow(LogLevel.Warn, true)]
        [DataRow(LogLevel.Info, true)]
        [DataRow(LogLevel.Always, true)]
        public void ShouldLogError(LogLevel filter, bool expected)
        {
            var didLog = false;
            this.debugLogger.LogLevelFilter = filter;
            this.debugLogger.LogError(l => (didLog = true).ToString());
            Assert.AreEqual(expected, didLog);
        }

        [TestMethod]
        [DataRow(LogLevel.Error, false)]
        [DataRow(LogLevel.Warn, true)]
        [DataRow(LogLevel.Info, true)]
        [DataRow(LogLevel.Always, true)]
        public void ShouldLogWarning(LogLevel filter, bool expected)
        {
            var didLog = false;
            this.debugLogger.LogLevelFilter = filter;
            this.debugLogger.LogWarning(l => (didLog = true).ToString());
            Assert.AreEqual(expected, didLog);
        }

        [TestMethod]
        [DataRow(LogLevel.Error, false)]
        [DataRow(LogLevel.Warn, false)]
        [DataRow(LogLevel.Info, true)]
        [DataRow(LogLevel.Always, true)]
        public void ShouldLogInfo(LogLevel filter, bool expected)
        {
            var didLog = false;
            this.debugLogger.LogLevelFilter = filter;
            this.debugLogger.LogInfo(l => (didLog = true).ToString());
            Assert.AreEqual(expected, didLog);
        }

        [TestMethod]
        [DataRow(LogLevel.Error, true)]
        [DataRow(LogLevel.Warn, true)]
        [DataRow(LogLevel.Info, true)]
        [DataRow(LogLevel.Always, true)]
        public void ShouldLogAlways(LogLevel filter, bool expected)
        {
            var didLog = false;
            this.debugLogger.LogLevelFilter = filter;
            this.debugLogger.LogAlways(l => (didLog = true).ToString());
            Assert.AreEqual(expected, didLog);
        }

        [TestMethod]
        public void FormatShouldReturnEmptyWhenDebuggerNotAttached()
        {
            var myLogger = new DebugLogger(false);
            var result = myLogger.Format("Testing Format var1: {0}; var2: {1};", 1, 2);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void FormatShouldReturnFormattedStringWhenDebuggerIsAttached()
        {
            var result = this.debugLogger.Format("Testing Format var1: {0}; var2: {1};", 1, 2);
            Assert.AreEqual("Testing Format var1: 1; var2: 2;", result);
        }

    }
}
