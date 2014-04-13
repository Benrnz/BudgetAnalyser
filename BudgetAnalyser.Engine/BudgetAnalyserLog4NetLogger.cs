using System;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A logger class that wraps Log4Net.
    ///     This class is designed to be a long-lived single instance that is passed around. If mulitple instances are used
    ///     IDisposable should be implemented.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetAnalyserLog4NetLogger : ILogger
    {
        private readonly ReaderWriterLockSlim alwaysLogLock = new ReaderWriterLockSlim();
        private readonly ILog log4NetLogger = LogManager.GetLogger("Budget Analyser Diagnostic Log");

        public BudgetAnalyserLog4NetLogger()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ConfigureLog4Net();
        }

        public void LogAlways(Func<string> logEntryBuilder)
        {
            Level currentLevel = GetCurrentLogLevel();
            this.alwaysLogLock.EnterWriteLock();
            try
            {
                SetLogLevelToAll();
                this.log4NetLogger.Info(logEntryBuilder());
            }
            finally
            {
                // Reset back
                SetLogLevel(currentLevel);
                this.alwaysLogLock.ExitWriteLock();
            }
        }

        public void LogError(Func<string> logEntryBuilder)
        {
            if (this.log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Error(logEntryBuilder()));
            }
        }

        public void LogError(Exception ex, Func<string> logEntryBuilder)
        {
            if (this.log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Error(logEntryBuilder(), ex));
            }
        }

        public void LogInfo(Func<string> logEntryBuilder)
        {
            if (this.log4NetLogger.IsInfoEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Info(logEntryBuilder()));
            }
        }

        public void LogWarning(Func<string> logEntryBuilder)
        {
            if (this.log4NetLogger.IsWarnEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Warn(logEntryBuilder()));
            }
        }

        protected virtual void ConfigureLog4Net()
        {
            XmlConfigurator.Configure();
        }

        private Level GetCurrentLogLevel()
        {
            var internalLogger = (Logger)this.log4NetLogger.Logger;
            return internalLogger.Level;
        }

        private void SetLogLevel(Level level)
        {
            var internalLogger = (Logger)this.log4NetLogger.Logger;
            internalLogger.Level = level;
        }

        private void SetLogLevelToAll()
        {
            var internalLogger = (Logger)this.log4NetLogger.Logger;
            internalLogger.Level = internalLogger.Hierarchy.LevelMap["ALL"];
        }

        private void SynchroniseWithAlwaysLog(Action action)
        {
            this.alwaysLogLock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                this.alwaysLogLock.ExitReadLock();
            }
        }
    }
}