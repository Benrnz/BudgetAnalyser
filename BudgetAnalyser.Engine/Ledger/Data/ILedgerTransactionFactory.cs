using System;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Factory to build <see cref="LedgerTransaction" />s based on a string transaction type name stored in DTO objects.
/// </summary>
internal interface ILedgerTransactionFactory
{
    /// <summary>
    ///     Builds the specified transaction.
    /// </summary>
    LedgerTransaction Build(string transactionTypeName, Guid id);
}
