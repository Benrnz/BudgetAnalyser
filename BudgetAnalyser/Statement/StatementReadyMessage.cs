using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     A message to notify interested parties that the statement model is ready for use.
    ///     Can be used to indicate a new statement model has been loaded.
    /// </summary>
    public class StatementReadyMessage : MessageBase
    {
        public StatementReadyMessage(StatementModel statement)
        {
            StatementModel = statement;
        }

        public StatementModel StatementModel { get; private set; }
    }
}
