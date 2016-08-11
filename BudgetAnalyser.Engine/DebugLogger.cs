using System;
using System.Diagnostics;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A logging implementation that simply outputs to the Debug stream <see cref="System.Diagnostics.Debug" />.
    /// </summary>
    [AutoRegisterWithIoC]
    public class DebugLogger : ILogger
    {
        /// <summary>
        ///     A custom string format method to avoid code translation and localisation warnings. The logging language is always
        ///     English.
        /// </summary>
        public string Format(string formatTemplate, params object[] parameters)
        {
            return string.Format(formatTemplate, parameters);
        }

        /// <summary>
        ///     Write a debug/diagnostic log entry regardless of the configured log level.
        /// </summary>
        public void LogAlways(Func<ILogger, string> logEntryBuilder)
        {
            Debug.WriteLine(ConstructLogEntry("ALWAYS", logEntryBuilder));
        }

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        public void LogError(Func<ILogger, string> logEntryBuilder)
        {
            Debug.WriteLine(ConstructLogEntry("ERROR", logEntryBuilder));
        }

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        public void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (logEntryBuilder == null) throw new ArgumentNullException(nameof(logEntryBuilder));

            Debug.WriteLine(ConstructLogEntry("ERROR", logEntryBuilder));
            Debug.WriteLine(ex.ToString());
        }

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to info.
        /// </summary>
        public void LogInfo(Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null) throw new ArgumentNullException(nameof(logEntryBuilder));
            Debug.WriteLine("INFO   " + logEntryBuilder(this));
        }

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to warning.
        /// </summary>
        public void LogWarning(Func<ILogger, string> logEntryBuilder)
        {
            Debug.WriteLine(ConstructLogEntry("WARN", logEntryBuilder));
        }

        private string ConstructLogEntry(string level, Func<ILogger, string> logEntryBuilder)
        {
            return $"{DateTime.Now.ToString("yy-MM-dThh:mm:ss.ffff")} {level.PadRight(7)}{logEntryBuilder(this)}";
        }
    }
}