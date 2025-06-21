using System.Diagnostics;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A logging implementation that simply outputs to the Debug stream <see cref="System.Diagnostics.Debug" />. Only if the debugger is attached, otherwise does nothing.
/// </summary>
public class DebugLogger : ILogger
{
    private readonly bool isDebuggerAttached;
    private LogLevel logLevelFilter = LogLevel.Always;

    /// <summary>
    ///     A logging implementation that simply outputs to the Debug stream <see cref="System.Diagnostics.Debug" />. Only if the debugger is attached, otherwise does nothing.
    /// </summary>
    public DebugLogger(bool? debuggerAttached = null)
    {
        this.isDebuggerAttached = debuggerAttached ?? Debugger.IsAttached;
    }

    public LogLevel LogLevelFilter
    {
        get => this.logLevelFilter;
        set
        {
            if (value == this.logLevelFilter)
            {
                return;
            }

            LogAlways(l => l.Format("Logging Filter Level change from {0} to {1}.", this.logLevelFilter, value));
            this.logLevelFilter = value;
        }
    }

    /// <summary>
    ///     A custom string format method to avoid code translation and localisation warnings. The logging language is always English.
    /// </summary>
    public string Format(string formatTemplate, params object?[] parameters)
    {
        return !this.isDebuggerAttached ? string.Empty : string.Format(formatTemplate, parameters);
    }

    /// <summary>
    ///     Write a debug/diagnostic log entry regardless of the configured log level.
    /// </summary>
    public void LogAlways(Func<ILogger, string> logEntryBuilder)
    {
        if (!ShouldILog(LogLevel.Always))
        {
            return;
        }

        var msg = ConstructLogEntry(LogLevel.Always, logEntryBuilder);
        Debug.WriteLine(msg);
    }

    /// <summary>
    ///     Write a debug/diagnostic log entry if the configured log level is set to error.
    /// </summary>
    public void LogError(Func<ILogger, string> logEntryBuilder)
    {
        if (!ShouldILog(LogLevel.Error))
        {
            return;
        }

        Debug.WriteLine(ConstructLogEntry(LogLevel.Error, logEntryBuilder));
    }

    /// <summary>
    ///     Write a debug/diagnostic log entry if the configured log level is set to error.
    /// </summary>
    public void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
    {
        if (!ShouldILog(LogLevel.Error))
        {
            return;
        }

        if (ex is null)
        {
            throw new ArgumentNullException(nameof(ex));
        }

        if (logEntryBuilder is null)
        {
            throw new ArgumentNullException(nameof(logEntryBuilder));
        }

        Debug.WriteLine(ConstructLogEntry(LogLevel.Error, logEntryBuilder));
        Debug.WriteLine(ex.ToString());
    }

    /// <summary>
    ///     Write a debug/diagnostic log entry if the configured log level is set to info.
    /// </summary>
    public void LogInfo(Func<ILogger, string> logEntryBuilder)
    {
        if (!ShouldILog(LogLevel.Info))
        {
            return;
        }

        if (logEntryBuilder is null)
        {
            throw new ArgumentNullException(nameof(logEntryBuilder));
        }

        Debug.WriteLine(ConstructLogEntry(LogLevel.Info, logEntryBuilder));
    }

    /// <summary>
    ///     Write a debug/diagnostic log entry if the configured log level is set to warning.
    /// </summary>
    public void LogWarning(Func<ILogger, string> logEntryBuilder)
    {
        if (!ShouldILog(LogLevel.Warn))
        {
            return;
        }

        Debug.WriteLine(ConstructLogEntry(LogLevel.Warn, logEntryBuilder));
    }

    private string ConstructLogEntry(LogLevel level, Func<ILogger, string> logEntryBuilder)
    {
        return $"{DateTime.Now:yy-MM-dThh:mm:ss.ffff} {level.ToString().ToUpperInvariant(),-7}{logEntryBuilder(this)}";
    }

    private bool ShouldILog(LogLevel logEntryLevel)
    {
        // Only log if debugger is attached.  Uses a field so unit testing can still test when debugger is not attached.
        if (!this.isDebuggerAttached)
        {
            return false;
        }

        if (LogLevelFilter == LogLevel.Always || logEntryLevel == LogLevel.Always)
        {
            return true;
        }

        switch (LogLevelFilter)
        {
            case LogLevel.Info:
                return true;
            case LogLevel.Warn:
                return logEntryLevel is LogLevel.Warn or LogLevel.Error;
            case LogLevel.Error:
                return logEntryLevel == LogLevel.Error;
            default:
                return false;
        }
    }
}
