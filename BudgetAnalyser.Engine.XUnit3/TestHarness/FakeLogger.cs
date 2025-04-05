using System.Diagnostics;
using System.Globalization;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

[AutoRegisterWithIoC(Named = "Named Logger", SingleInstance = true)]
public class FakeLogger : ILogger
{
    public LogLevel LogLevelFilter { get; set; }

    public string Format(string format, params object[] parameters)
    {
        return string.Format(CultureInfo.InvariantCulture, format, parameters);
    }

    public virtual void LogAlways(Func<ILogger, string> logEntryBuilder)
    {
        Debug.Write("LOG ALWAYS:");
        Debug.WriteLine(logEntryBuilder(this));
    }

    public virtual void LogError(Func<ILogger, string> logEntryBuilder)
    {
        Debug.Write("ERROR:");
        Debug.WriteLine(logEntryBuilder(this));
    }

    public virtual void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
    {
        Debug.Write("ERROR:");
        Debug.WriteLine(logEntryBuilder(this));
        Debug.WriteLine(ex.ToString());
    }

    public virtual void LogInfo(Func<ILogger, string> logEntryBuilder)
    {
        Debug.Write("LOG INFO:");
        Debug.WriteLine(logEntryBuilder(this));
    }

    public virtual void LogWarning(Func<ILogger, string> logEntryBuilder)
    {
        Debug.Write("LOG WARNING:");
        Debug.WriteLine(logEntryBuilder(this));
    }
}
