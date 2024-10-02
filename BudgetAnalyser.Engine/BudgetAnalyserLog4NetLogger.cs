using JetBrains.Annotations;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A logger class that wraps Log4Net.
///     This class is designed to be a long-lived single instance that is passed around. If mulitple instances are used
///     IDisposable should be implemented.
/// </summary>
[UsedImplicitly]
public class BudgetAnalyserLog4NetLogger : DebugLogger
{
    // TODO This class needs to be redesigned to use something other than Log4Net.
}