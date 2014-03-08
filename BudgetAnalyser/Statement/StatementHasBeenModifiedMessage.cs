using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Statement
{
    public class StatementHasBeenModifiedMessage : MessageBase
    {
        public StatementHasBeenModifiedMessage(bool dirty)
        {
            Dirty = dirty;
        }

        public StatementHasBeenModifiedMessage(bool dirty, StatementModel statementModel) : this(dirty)
        {
            StatementModel = statementModel;
        }

        public bool Dirty { get; private set; }

        public StatementModel StatementModel { get; private set; }
    }
}