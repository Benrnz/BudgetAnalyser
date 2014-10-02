using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Factory to build <see cref="LedgerTransaction"/>s based on a string transaction type name stored in DTO objects.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerTransactionFactory : ILedgerTransactionFactory
    {
        public LedgerTransaction Build(string transactionTypeName, Guid id)
        {
            var type = Type.GetType(transactionTypeName);
            if (type == null)
            {
                throw new FileFormatException("Invalid transaction type encountered: " + transactionTypeName);
            }

            return Activator.CreateInstance(type, id) as LedgerTransaction;
        }
    }
}
