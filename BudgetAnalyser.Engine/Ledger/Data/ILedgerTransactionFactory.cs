using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerTransactionFactory
    {
        LedgerTransaction Build(string transactionTypeName, Guid id);
    }
}