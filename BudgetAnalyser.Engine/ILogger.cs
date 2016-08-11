using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A generic delayed logging interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     A custom string format method to avoid code translation and localisation warnings. The logging language is always
        ///     English.
        /// </summary>
        string Format([NotNull] string formatTemplate, params object[] parameters);

        /// <summary>
        ///     Write a debug/diagnostic log entry regardless of the configured log level.
        /// </summary>
        void LogAlways([NotNull] Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        void LogError([NotNull] Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        void LogError([NotNull] Exception ex, [NotNull] Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to info.
        /// </summary>
        void LogInfo([NotNull] Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to warning.
        /// </summary>
        void LogWarning([NotNull] Func<ILogger, string> logEntryBuilder);
    }
}