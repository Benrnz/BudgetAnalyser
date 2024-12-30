using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     A message to notify interested parties that the transaction data has been modified.
    ///     This includes data edits, deletions, and adding new transactions to the collection.
    /// </summary>
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