namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A generic delayed logging interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///    The current log level filter. Only log entries with a level equal to or higher than this level will be logged.
        /// </summary>
        public LogLevel LogLevelFilter { get; set; }

        /// <summary>
        ///     A custom string format method to avoid code translation and localisation warnings. The logging language is always
        ///     English.
        /// </summary>
        string Format(string formatTemplate, params object?[] parameters);

        /// <summary>
        ///     Write a debug/diagnostic log entry regardless of the configured log level.
        /// </summary>
        void LogAlways(Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        void LogError(Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to error.
        /// </summary>
        void LogError(Exception ex, Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to info.
        /// </summary>
        void LogInfo(Func<ILogger, string> logEntryBuilder);

        /// <summary>
        ///     Write a debug/diagnostic log entry if the configured log level is set to warning.
        /// </summary>
        void LogWarning(Func<ILogger, string> logEntryBuilder);
    }
}
