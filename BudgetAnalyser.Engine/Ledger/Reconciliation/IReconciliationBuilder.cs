using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     Responsible for reconciling the month's transactions against the budget and creating a
    ///     <see cref="LedgerEntryLine" /> that shows the results.
    /// </summary>
    internal interface IReconciliationBuilder
    {
        /// <summary>
        ///     The <see cref="LedgerBook" /> that we are building a monthly reconciliation for. This property must be set prior to
        ///     calling <see cref="CreateNewMonthlyReconciliation" /> otherwise a
        ///     <see cref="ArgumentException" /> will be thrown.
        /// </summary>
        LedgerBook LedgerBook { get; set; }

        /// <summary>
        ///     Creates a new monthly reconciliation the the given <see cref="LedgerBook" /> for the current month.
        /// </summary>
        /// <returns>A newly created and populated <see cref="LedgerEntryLine" />.</returns>
        ReconciliationResult CreateNewMonthlyReconciliation(
            DateTime reconciliationDateExclusive,
            [NotNull] BudgetModel budget,
            [NotNull] StatementModel statement,
            [NotNull] params BankBalance[] bankBalances);
    }
}