using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class XUnitLogger(ITestOutputHelper outputWriter) : FakeLogger
{
    private readonly ITestOutputHelper outputWriter = outputWriter;

    public override void LogAlways(Func<ILogger, string> logEntryBuilder)
    {
        base.LogAlways(logEntryBuilder);
        this.outputWriter.WriteLine(logEntryBuilder(this));
    }

    public override void LogError(Func<ILogger, string> logEntryBuilder)
    {
        base.LogError(logEntryBuilder);
        this.outputWriter.WriteLine(logEntryBuilder(this));
    }

    public override void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
    {
        base.LogError(ex, logEntryBuilder);
        this.outputWriter.WriteLine("ERROR:");
        this.outputWriter.WriteLine(logEntryBuilder(this));
        this.outputWriter.WriteLine(ex.ToString());
    }

    public override void LogInfo(Func<ILogger, string> logEntryBuilder)
    {
        base.LogInfo(logEntryBuilder);
        this.outputWriter.WriteLine(logEntryBuilder(this));
    }

    public override void LogWarning(Func<ILogger, string> logEntryBuilder)
    {
        base.LogWarning(logEntryBuilder);
        this.outputWriter.WriteLine(logEntryBuilder(this));
    }
}
