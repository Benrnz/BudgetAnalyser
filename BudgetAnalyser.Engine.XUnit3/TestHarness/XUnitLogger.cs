namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class XUnitLogger(ITestOutputHelper outputWriter) : FakeLogger
{
    public override void LogAlways(Func<ILogger, string> logEntryBuilder)
    {
        base.LogAlways(logEntryBuilder);
        outputWriter.WriteLine($"{DateTime.Now.Ticks} {logEntryBuilder(this)}");
    }

    public override void LogError(Func<ILogger, string> logEntryBuilder)
    {
        base.LogError(logEntryBuilder);
        outputWriter.WriteLine($"{DateTime.Now.Ticks} {logEntryBuilder(this)}");
    }

    public override void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
    {
        base.LogError(ex, logEntryBuilder);
        outputWriter.WriteLine("ERROR:");
        outputWriter.WriteLine($"{DateTime.Now.Ticks} {logEntryBuilder(this)}");
        outputWriter.WriteLine(ex.ToString());
    }

    public override void LogInfo(Func<ILogger, string> logEntryBuilder)
    {
        base.LogInfo(logEntryBuilder);
        outputWriter.WriteLine($"{DateTime.Now.Ticks} {logEntryBuilder(this)}");
    }

    public override void LogWarning(Func<ILogger, string> logEntryBuilder)
    {
        base.LogWarning(logEntryBuilder);
        outputWriter.WriteLine($"{DateTime.Now.Ticks} {logEntryBuilder(this)}");
    }
}
