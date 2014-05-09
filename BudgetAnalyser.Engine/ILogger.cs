using System;

namespace BudgetAnalyser.Engine
{
    public interface ILogger
    {
        string Format(string formatTemplate, params object[] parameters);
        void LogAlways(Func<string> logEntryBuilder);
        void LogError(Func<string> logEntryBuilder);
        void LogError(Exception ex, Func<string> logEntryBuilder);
        void LogInfo(Func<string> logEntryBuilder);
        void LogWarning(Func<string> logEntryBuilder);
    }
}