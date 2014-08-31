using System;

namespace BudgetAnalyser.Engine
{
    public class NullLogger : ILogger
    {
        public string Format(string formatTemplate, params object[] parameters)
        {
            return null;
        }

        public void LogAlways(Func<ILogger, string> logEntryBuilder)
        {
        }

        public void LogError(Func<ILogger, string> logEntryBuilder)
        {
        }

        public void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
        {
        }

        public void LogInfo(Func<ILogger, string> logEntryBuilder)
        {
        }

        public void LogWarning(Func<ILogger, string> logEntryBuilder)
        {
        }
    }
}
