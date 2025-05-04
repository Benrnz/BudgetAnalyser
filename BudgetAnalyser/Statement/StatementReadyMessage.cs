using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Statement;

/// <summary>
///     A message to notify interested parties that the statement model is ready for use.  Can be used to indicate a new statement model has been loaded.
/// </summary>
public class StatementReadyMessage : MessageBase
{
    public StatementReadyMessage(StatementModel? statement)
    {
        StatementModel = statement;
    }

    /// <summary>
    ///     The new statement model.  This may be null if the statement is closed.
    /// </summary>
    public StatementModel? StatementModel { get; private set; }
}
