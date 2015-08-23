using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     Responsible for reconciliation the month's transactions against the budget and creating a
    ///     <see cref="LedgerEntryLine" /> that shows the results.
    /// </summary>
    public interface IReconciliationBuilder
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
        LedgerEntryLine CreateNewMonthlyReconciliation(
            DateTime reconciliationDateExcl,
            [NotNull] IEnumerable<BankBalance> bankBalances,
            [NotNull] BudgetModel budget,
            [NotNull] StatementModel statement,
            [NotNull] ToDoCollection toDoList);
    }
}