using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A generic delayed logging interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// A custom string format method to avoid code translation and localisation warnings. The logging language is always English.
        /// </summary>
        string Format(string formatTemplate, params object[] parameters);

        void LogAlways(Func<string> logEntryBuilder);
        void LogError(Func<string> logEntryBuilder);
        void LogError(Exception ex, Func<string> logEntryBuilder);
        void LogInfo(Func<string> logEntryBuilder);
        void LogWarning(Func<string> logEntryBuilder);
    }
}