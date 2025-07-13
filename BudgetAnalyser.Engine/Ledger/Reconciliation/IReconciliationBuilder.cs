using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <summary>
///     Responsible for reconciling the period's transactions against the budget and creating a <see cref="LedgerEntryLine" /> that shows the results.
///     The <see cref="LedgerEntryLine" /> closes off the period just completed, and shows balances for the next.
/// </summary>
internal interface IReconciliationBuilder
{
    /// <summary>
    ///     The <see cref="LedgerBook" /> that we are building a reconciliation for. This property must be set prior to
    ///     calling <see cref="CreateNewMonthlyReconciliation" /> otherwise an <see cref="ArgumentException" /> will be thrown.
    /// </summary>
    LedgerBook? LedgerBook { get; set; }

    /// <summary>
    ///     Creates a new reconciliation for the given <see cref="LedgerBook" /> for the current period.
    /// </summary>
    /// <returns>A newly created and populated <see cref="LedgerEntryLine" />.</returns>
    ReconciliationResult CreateNewMonthlyReconciliation(
        DateOnly reconciliationClosingDateExclusive,
        BudgetModel budget,
        TransactionSetModel transactionSet,
        params BankBalance[] bankBalances);
}
