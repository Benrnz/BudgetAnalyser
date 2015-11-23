using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    public interface IReconciliationManager
    {
        /// <summary>
        ///     Creates a new LedgerEntryLine for a <see cref="LedgerBook" /> to begin reconciliation.
        /// </summary>
        /// <param name="ledgerBook">The ledger book that this reconciliation belongs to</param>
        /// <param name="reconciliationDate">
        ///     The startDate for the <see cref="LedgerEntryLine" />. This is usually the previous Month's "Reconciliation-Date",
        ///     as this month's reconciliation starts with this date and includes transactions
        ///     from that date. This date is different to the "Reconciliation-Date" that appears next to the resulting
        ///     reconciliation which is the end date for the period.
        /// </param>
        /// <param name="budgetContext">The current budget.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <param name="currentBankBalances">
        ///     The bank balances as at the reconciliation date to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="budgetContext" /> is in an invalid state, ie the
        ///     supplied budget is not active.
        /// </exception>
        /// <exception cref="ValidationWarningException">
        ///     Thrown when there are outstanding validation errors in the
        ///     <paramref name="statement" />, <paramref name="budgetContext" />, or <paramref name="ledgerBook" />.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the supplied dates are invalid or not consistent with the
        ///     <paramref name="ledgerBook" />.
        /// </exception>
        ReconciliationResult MonthEndReconciliation(
            [NotNull] LedgerBook ledgerBook,
            DateTime reconciliationDate,
            [NotNull] IBudgetCurrencyContext budgetContext,
            [NotNull] StatementModel statement,
            bool ignoreWarnings,
            [NotNull] params BankBalance[] currentBankBalances);

        /// <summary>
        ///     Performs a funds transfer for the given ledger entry line.
        /// </summary>
        void TransferFunds([NotNull] TransferFundsCommand transferDetails, [NotNull] LedgerEntryLine ledgerEntryLine);

        /// <summary>
        ///     Examines the ledger book's most recent reconciliation looking for transactions waiting to be matched to
        ///     transactions imported in the current month.
        ///     If any transactions are found, the statement is then examined to see if the transactions appear, if they do not a
        ///     new <see cref="ValidationWarningException" />
        ///     is thrown; otherwise the method returns.
        /// </summary>
        void ValidateAgainstOrphanedAutoMatchingTransactions(LedgerBook ledgerBook, StatementModel statement);
    }
}