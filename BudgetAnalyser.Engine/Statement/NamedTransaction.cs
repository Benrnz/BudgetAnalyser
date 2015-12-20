using System;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A simple type to represent the transaction type classification provided by the bank for a transaction.
    ///     The type code is simply represented as a string.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Statement.TransactionType" />
    public class NamedTransaction : TransactionType
    {
        private readonly string name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamedTransaction" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public NamedTransaction(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.name = name;

            // TODO remove Sign
            // Sign = sign;
        }

        /// <summary>
        ///     Gets the transaction type name.
        /// </summary>
        public override string Name => this.name;

        ///// <summary>
        ///// Gets the sign.
        ///// </summary>
        //public decimal Sign { get; private set; }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}