using Shouldly;

namespace BudgetAnalyser.Engine.XUnit;

public class DebugLoggerTest
{
    private readonly ILogger debugLogger = new DebugLogger(true);

    [Fact]
    public void FormatShouldReturnEmptyWhenDebuggerNotAttached()
    {
        var myLogger = new DebugLogger(false);
        var result = myLogger.Format("Testing Format var1: {0}; var2: {1};", 1, 2);
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void FormatShouldReturnFormattedStringWhenDebuggerIsAttached()
    {
        var result = this.debugLogger.Format("Testing Format var1: {0}; var2: {1};", 1, 2);
        result.ShouldBe("Testing Format var1: 1; var2: 2;");
    }

    [Fact]
    public void LoggingShouldNotEvalStringInterolationWhenNotLogging()
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = LogLevel.Error;
        this.debugLogger.LogInfo(_ => $"My string {(didLog = true).ToString()}");
        didLog.ShouldBeFalse();
    }

    [Fact]
    public void NoLoggingShouldOccurWhenDebuggerNotAttached()
    {
        var logger = new DebugLogger(false);
        var didLog = false;
        logger.LogError(l => (didLog = true).ToString());
        logger.LogWarning(l => (didLog = true).ToString());
        logger.LogInfo(l => (didLog = true).ToString());
        logger.LogAlways(l => (didLog = true).ToString());
        didLog.ShouldBeFalse();
    }

    [Fact]
    public void ShouldInitialiseToAlwaysLog()
    {
        this.debugLogger.LogLevelFilter.ShouldBe(LogLevel.Always);
    }

    [Theory]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Warn, true)]
    [InlineData(LogLevel.Info, true)]
    [InlineData(LogLevel.Always, true)]
    public void ShouldLogAlways(LogLevel filter, bool expected)
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = filter;
        this.debugLogger.LogAlways(l => (didLog = true).ToString());
        didLog.ShouldBe(expected);
    }

    [Theory]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Warn, true)]
    [InlineData(LogLevel.Info, true)]
    [InlineData(LogLevel.Always, true)]
    public void ShouldLogError(LogLevel filter, bool expected)
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = filter;
        this.debugLogger.LogError(l => (didLog = true).ToString());
        didLog.ShouldBe(expected);
    }

    [Theory]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Warn, true)]
    [InlineData(LogLevel.Info, true)]
    [InlineData(LogLevel.Always, true)]
    public void ShouldLogExceptionWhenFilteredToError(LogLevel filter, bool expected)
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = filter;
        this.debugLogger.LogError(new Exception("Test Exception"), l => (didLog = true).ToString());
        didLog.ShouldBe(expected);
    }

    [Theory]
    [InlineData(LogLevel.Error, false)]
    [InlineData(LogLevel.Warn, false)]
    [InlineData(LogLevel.Info, true)]
    [InlineData(LogLevel.Always, true)]
    public void ShouldLogInfo(LogLevel filter, bool expected)
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = filter;
        this.debugLogger.LogInfo(l => (didLog = true).ToString());
        didLog.ShouldBe(expected);
    }

    [Theory]
    [InlineData(LogLevel.Error, false)]
    [InlineData(LogLevel.Warn, true)]
    [InlineData(LogLevel.Info, true)]
    [InlineData(LogLevel.Always, true)]
    public void ShouldLogWarning(LogLevel filter, bool expected)
    {
        var didLog = false;
        this.debugLogger.LogLevelFilter = filter;
        this.debugLogger.LogWarning(l => (didLog = true).ToString());
        didLog.ShouldBe(expected);
    }

    [Fact]
    public void ShouldRememberLogFilterChanges()
    {
        this.debugLogger.LogLevelFilter = LogLevel.Error;
        this.debugLogger.LogLevelFilter.ShouldBe(LogLevel.Error);
    }
}
