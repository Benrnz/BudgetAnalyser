using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
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
