using System.Diagnostics;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     Represents any transaction type that could be used on a transaction. These type codes are provided by the bank.
    /// </summary>
    [DebuggerDisplay("TransactionType {Name}")]
    public abstract class TransactionType
    {
        /// <summary>
        ///     Gets the transaction type name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}