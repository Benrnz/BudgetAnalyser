using System;
using System.Globalization;
using System.Threading;
using BudgetAnalyser.Engine.Annotations;
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
    public class BudgetAnalyserLog4NetLogger : ILogger, IDisposable
    {
        private readonly ReaderWriterLockSlim alwaysLogLock = new ReaderWriterLockSlim();
        private readonly ILog log4NetLogger = LogManager.GetLogger("Budget Analyser Diagnostic Log");
        private bool disposed = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification="Reviewed, ok here, required for testing")]
        public BudgetAnalyserLog4NetLogger()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ConfigureLog4Net();
        }

        ~BudgetAnalyserLog4NetLogger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Format(string formatTemplate, params object[] parameters)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            return string.Format(CultureInfo.CurrentCulture, formatTemplate, parameters);
        }

        public void LogAlways([NotNull] Func<string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

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

        public void LogError([NotNull] Func<string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }
            if (this.log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Error(logEntryBuilder()));
            }
        }

        public void LogError(Exception ex, [NotNull] Func<string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (this.log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Error(logEntryBuilder(), ex));
            }
        }

        public void LogInfo([NotNull] Func<string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (this.log4NetLogger.IsInfoEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Info(logEntryBuilder()));
            }
        }

        public void LogWarning([NotNull] Func<string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (this.log4NetLogger.IsWarnEnabled)
            {
                SynchroniseWithAlwaysLog(() => this.log4NetLogger.Warn(logEntryBuilder()));
            }
        }

        protected virtual void ConfigureLog4Net()
        {
            XmlConfigurator.Configure();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.alwaysLogLock.Dispose();
            }
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