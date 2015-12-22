using BudgetAnalyser.Engine;
using log4net;
using log4net.Core;
using Moq;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class LoggerTestHarness : BudgetAnalyserLog4NetLogger
    {
        public LoggerTestHarness(Mock<ILog> mockInternalLogger)
        {
            Log4NetLoggerMock = mockInternalLogger;
            ConfigureLog4Net();
        }

        public bool ConfigWasCalled { get; private set; }
        public bool HasInternalLogger => Log4NetLogger != null;
        public Mock<ILog> Log4NetLoggerMock { get; set; }
        public Level LoggingLevel { get; private set; } = Level.Info;

        protected override Level CurrentLogLevel
        {
            get { return LoggingLevel; }
            set { LoggingLevel = value; }
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

        protected override void SetLogLevelToAll()
        {
            LoggingLevel = Level.All;
        }
    }
}