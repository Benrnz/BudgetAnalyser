using System;

namespace BudgetAnalyser.Engine
{
    public interface ILogger
    {
        void LogInfo(Func<string> logEntryBuilder);
        void LogWarning(Func<string> logEntryBuilder);
        void LogError(Func<string> logEntryBuilder);
        void LogError(Exception ex, Func<string> logEntryBuilder);
        void LogAlways(Func<string> logEntryBuilder);
    }
}
