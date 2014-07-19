using BudgetAnalyser.Engine;
using log4net;
using log4net.Core;
using Moq;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class LoggerTestHarness : BudgetAnalyserLog4NetLogger
    {
        private Level loggingLevel = Level.Info;

        public LoggerTestHarness(Mock<ILog> mockInternalLogger)
        {
            Log4NetLoggerMock = mockInternalLogger;
            ConfigureLog4Net();
        }

        public bool ConfigWasCalled { get; private set; }

        public bool HasInternalLogger
        {
            get { return Log4NetLogger != null; }
        }

        public Mock<ILog> Log4NetLoggerMock { get; set; }

        public Level LoggingLevel
        {
            get { return this.loggingLevel; }
        }

        protected override void ConfigureLog4Net()
        {
            if (Log4NetLoggerMock == null)
            {
                Log4NetLoggerMock = new Mock<ILog>();
            }

            Log4NetLogger = Log4NetLoggerMock.Object;
            ConfigWasCalled = true;
        }

        protected override Level GetCurrentLogLevel()
        {
            return this.loggingLevel;
        }

        protected override void SetLogLevel(Level level)
        {
            this.loggingLevel = level;
        }

        protected override void SetLogLevelToAll()
        {
            this.loggingLevel = Level.All;
        }
    }
}