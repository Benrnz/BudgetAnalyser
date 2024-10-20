using System.Diagnostics;
using System.Globalization;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Wpf.UnitTest
{
    [AutoRegisterWithIoC(Named = "Named Logger", SingleInstance  = true)]
    public class FakeLogger : ILogger
    {
        public LogLevel LogLevelFilter { get; set; }

        public string Format(string format, params object[] parameters)
        {
            return string.Format(CultureInfo.InvariantCulture, format, parameters);
        }

        public void LogAlways(Func<ILogger, string> logEntryBuilder)
        {
            Debug.Write("LOG ALWAYS:");
            Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogError(Func<ILogger, string> logEntryBuilder)
        {
            Debug.Write("ERROR:");
            Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
        {
            Debug.Write("ERROR:");
            Debug.WriteLine(logEntryBuilder(this));
            Debug.WriteLine(ex.ToString());
        }

        public void LogInfo(Func<ILogger, string> logEntryBuilder)
        {
            Debug.Write("LOG INFO:");
            Debug.WriteLine(logEntryBuilder(this));
        }

        public void LogWarning(Func<ILogger, string> logEntryBuilder)
        {
            Debug.Write("LOG WARNING:");
            Debug.WriteLine(logEntryBuilder(this));
        }
    }
}