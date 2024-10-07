// TODO Reconsider if this is necessary, I'm removing Log4Net from the project.

//using System;
//using JetBrains.Annotations;

//namespace BudgetAnalyser.Engine;

///// <summary>
///// An aggregate class that combines <see cref="DebugLogger"/> and <see cref="BudgetAnalyserLog4NetLogger"/> so both are output to.
///// This class is intended to be a singleton.
///// </summary>
//[UsedImplicitly]
//public class DebugPlusLog4NetLogger : BudgetAnalyserLog4NetLogger, IDisposable
//{
//    private readonly ILogger debugLogger = new DebugLogger();

//    public override void LogAlways(Func<ILogger, string> logEntryBuilder)
//    {
//        this.debugLogger.LogAlways(logEntryBuilder);
//        base.LogAlways(logEntryBuilder);
//    }

//    public override void LogError(Func<ILogger, string> logEntryBuilder)
//    {
//        this.debugLogger.LogError(logEntryBuilder);
//        base.LogError(logEntryBuilder);
//    }

//    public override void LogError(Exception ex, Func<ILogger, string> logEntryBuilder)
//    {
//        this.debugLogger.LogError(ex, logEntryBuilder);
//        base.LogError(ex, logEntryBuilder);
//    }

//    public override void LogInfo(Func<ILogger, string> logEntryBuilder)
//    {
//        this.debugLogger.LogInfo(logEntryBuilder);
//        base.LogInfo(logEntryBuilder);
//    }

//    public override void LogWarning(Func<ILogger, string> logEntryBuilder)
//    {
//        this.debugLogger.LogWarning(logEntryBuilder);
//        base.LogWarning(logEntryBuilder);
//    }
//}

namespace BudgetAnalyser.Engine;