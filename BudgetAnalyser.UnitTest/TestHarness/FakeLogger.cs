using System;
using System.Globalization;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class FakeLogger : ILogger
    {
        public void LogInfo(Func<ILogger, string> logEntryBuilder)
        {
            System.Diagnostics.Debug.Write("LOG INFO:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogWarning(Func<ILogger, string> logEntryBuilder)
        {
            System.Diagnostics.Debug.Write("LOG WARNING:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogError(Func<ILogger, string> logEntryBuilder)
        {
            System.Diagnostics.Debug.Write("ERROR:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
        {
            System.Diagnostics.Debug.Write("ERROR:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder(this));
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        public void LogAlways(Func<ILogger, string> logEntryBuilder)
        {
            System.Diagnostics.Debug.Write("LOG ALWAYS:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder(this));
        }

        public string Format(string format, params object[] parameters)
        {
            return string.Format(CultureInfo.InvariantCulture, format, parameters);
        }
    }
}
