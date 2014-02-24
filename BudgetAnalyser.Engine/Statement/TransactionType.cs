using System.Diagnostics;

namespace BudgetAnalyser.Engine.Statement
{
    [DebuggerDisplay("TransactionType {Name}")]
    public abstract class TransactionType
    {
        public abstract string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}