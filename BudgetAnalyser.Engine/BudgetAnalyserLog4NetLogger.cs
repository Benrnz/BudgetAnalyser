using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using JetBrains.Annotations;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A logger class that wraps Log4Net.
///     This class is designed to be a long-lived single instance that is passed around. If mulitple instances are used
///     IDisposable should be implemented.
/// </summary>
[UsedImplicitly]
public class BudgetAnalyserLog4NetLogger : ILogger, IDisposable
{
    private readonly ReaderWriterLockSlim alwaysLogLock = new();
    private bool disposed;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed, ok here, required for testing")]
    public BudgetAnalyserLog4NetLogger()
    {
        // ReSharper disable once DoNotCallOverridableMethodsInConstructor
        // Ok here, required for testing
        ConfigureLog4Net();
    }

    private Level CurrentLogLevel
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

    private ILog Log4NetLogger { get; set; }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string Format(string formatTemplate, params object[] parameters)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
        }

        return string.Format(CultureInfo.CurrentCulture, formatTemplate, parameters);
    }

    public virtual void LogAlways([NotNull] Func<ILogger, string> logEntryBuilder)
    {
        if (logEntryBuilder == null)
        {
            throw new ArgumentNullException(nameof(logEntryBuilder));
        }

        if (this.disposed)
        {
            throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
        }

        var currentLevel = CurrentLogLevel;
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

    public virtual void LogError([NotNull] Func<ILogger, string> logEntryBuilder)
    {
        if (Log4NetLogger.IsErrorEnabled)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException(nameof(logEntryBuilder));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            SynchroniseWithAlwaysLog(() => Log4NetLogger.Error(logEntryBuilder(this)));
        }
    }

    public virtual void LogError(Exception ex, [NotNull] Func<ILogger, string> logEntryBuilder)
    {
        if (Log4NetLogger.IsErrorEnabled)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException(nameof(logEntryBuilder));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            SynchroniseWithAlwaysLog(() => Log4NetLogger.Error(logEntryBuilder(this), ex));
        }
    }

    public virtual void LogInfo([NotNull] Func<ILogger, string> logEntryBuilder)
    {
        if (Log4NetLogger.IsInfoEnabled)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException(nameof(logEntryBuilder));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            SynchroniseWithAlwaysLog(() => Log4NetLogger.Info(logEntryBuilder(this)));
        }
    }

    public virtual void LogWarning([NotNull] Func<ILogger, string> logEntryBuilder)
    {
        if (Log4NetLogger.IsWarnEnabled)
        {
            if (logEntryBuilder == null)
            {
                throw new ArgumentNullException(nameof(logEntryBuilder));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("BudgetAnalyserLog4NetLogger");
            }

            SynchroniseWithAlwaysLog(() => Log4NetLogger.Warn(logEntryBuilder(this)));
        }
    }

    private void ConfigureLog4Net()
    {
        Log4NetLogger = LogManager.GetLogger("Budget Analyser Diagnostic Log");
        XmlConfigurator.Configure();
    }

    private void Dispose(bool disposing)
    {
        this.disposed = true;
        if (disposing)
        {
            this.alwaysLogLock.Dispose();
        }
    }

    private void SetLogLevelToAll()
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

    ~BudgetAnalyserLog4NetLogger()
    {
        Dispose(false);
    }
}