using System;
using System.IO;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class FakeLogger : ILogger
    {
        public void LogInfo(Func<string> logEntryBuilder)
        {
            System.Diagnostics.Debug.WriteLine("LOG INFO:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder());
        }

        public void LogWarning(Func<string> logEntryBuilder)
        {
            System.Diagnostics.Debug.WriteLine("LOG WARNING:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder());
        }

        public void LogError(Func<string> logEntryBuilder)
        {
            System.Diagnostics.Debug.WriteLine("ERROR:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder());
        }

        public void LogError(Exception ex, Func<string> logEntryBuilder)
        {
            System.Diagnostics.Debug.WriteLine("ERROR:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        public void LogAlways(Func<string> logEntryBuilder)
        {
            System.Diagnostics.Debug.WriteLine("LOG ALWAYS:");
            System.Diagnostics.Debug.WriteLine(logEntryBuilder());
        }
    }
}
