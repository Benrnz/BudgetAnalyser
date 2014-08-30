using System;
using System.Diagnostics.CodeAnalysis;
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
        private bool disposed;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed, ok here, required for testing")]
        public BudgetAnalyserLog4NetLogger()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            // Ok here, required for testing
            ConfigureLog4Net();
        }

        ~BudgetAnalyserLog4NetLogger()
        {
            Dispose(false);
        }

        protected virtual Level CurrentLogLevel
        {
            get
            {
                var internalLogger = (Logger)Log4NetLogger.Logger;
                return internalLogger.Level;
            }
            set
            {
                var internalLogger = (Logger)Log4NetLogger.Logger;
                internalLogger.Level = value;
            }
        }

        protected ILog Log4NetLogger { get; set; }

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

        public void LogAlways([NotNull] Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            Level currentLevel = CurrentLogLevel;
            this.alwaysLogLock.EnterWriteLock();
            try
            {
                SetLogLevelToAll();
                Log4NetLogger.Info(logEntryBuilder(this));
            }
            finally
            {
                // Reset back
                CurrentLogLevel = currentLevel;
                this.alwaysLogLock.ExitWriteLock();
            }
        }

        public void LogError([NotNull] Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (Log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => Log4NetLogger.Error(logEntryBuilder(this)));
            }
        }

        public void LogError(Exception ex, [NotNull] Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (Log4NetLogger.IsErrorEnabled)
            {
                SynchroniseWithAlwaysLog(() => Log4NetLogger.Error(logEntryBuilder(this), ex));
            }
        }

        public void LogInfo([NotNull] Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (Log4NetLogger.IsInfoEnabled)
            {
                SynchroniseWithAlwaysLog(() => Log4NetLogger.Info(logEntryBuilder(this)));
            }
        }

        public void LogWarning([NotNull] Func<ILogger, string> logEntryBuilder)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException("logEntryBuilder");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            if (Log4NetLogger.IsWarnEnabled)
            {
                SynchroniseWithAlwaysLog(() => Log4NetLogger.Warn(logEntryBuilder(this)));
            }
        }

        protected virtual void ConfigureLog4Net()
        {
            Log4NetLogger = LogManager.GetLogger("Budget Analyser Diagnostic Log");
            XmlConfigurator.Configure();
        }

        protected virtual void Dispose(bool disposing)
        {
            this.disposed = true;
            if (disposing)
            {
                this.alwaysLogLock.Dispose();
            }
        }

        protected virtual void SetLogLevelToAll()
        {
            var internalLogger = (Logger)Log4NetLogger.Logger;
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