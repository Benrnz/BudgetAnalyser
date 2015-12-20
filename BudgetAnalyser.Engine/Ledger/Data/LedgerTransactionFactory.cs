using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Factory to build <see cref="LedgerTransaction" />s based on a string transaction type name stored in DTO objects.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class LedgerTransactionFactory : ILedgerTransactionFactory
    {
        /// <summary>
        ///     Builds the specified transaction type name.
        /// </summary>
        /// <param name="transactionTypeName">Name of the transaction type.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="DataFormatException">Invalid transaction type encountered:  + transactionTypeName</exception>
        public LedgerTransaction Build(string transactionTypeName, Guid id)
        {
            var type = Type.GetType(transactionTypeName);
            if (type == null)
            {
                throw new DataFormatException("Invalid transaction type encountered: " + transactionTypeName);
            }

            return Activator.CreateInstance(type, id) as LedgerTransaction;
        }
    }
}