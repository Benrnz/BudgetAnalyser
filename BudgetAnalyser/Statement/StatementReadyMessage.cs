using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Statement
{
    public class StatementReadyMessage : MessageBase
    {
        public StatementReadyMessage(StatementModel statement)
        {
            StatementModel = statement;
        }

        public StatementModel StatementModel { get; private set; }
    }
}