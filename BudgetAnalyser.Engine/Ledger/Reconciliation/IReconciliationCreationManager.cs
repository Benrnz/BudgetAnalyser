using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    internal interface IReconciliationCreationManager
    {
        /// <summary>
        ///     Processes a new period end reconciliation and creates a new LedgerEntryLine and adds it to the <see cref="LedgerBook" />.
        ///     All necessary activities are managed by calling this top level method.
        /// </summary>
        /// <param name="ledgerBook">The ledger book that this reconciliation belongs to</param>
        /// <param name="closingDateExclusive">
        ///     The closing off date of the period performing a reconciliation for. This is a fixed period size relative to the previous <see cref="LedgerEntryLine"/>.  For example, for monthly budget
        ///     usually the previous Month's "Reconciliation-Date" plus one month. If pay day is 20th each month, and the period is 20-Apr to 19-May, the <paramref name="closingDateExclusive"/> would
        ///     be 20-May. This will select transactions based on transaction date less than 20-May.  The <see cref="LedgerEntryLine.Date"/> will also be stamped 20-May.   
        /// </param>
        /// <param name="budgetCollection">The collection of budgets. The Reconciliation engine classes will make a decision which budget to choose.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <param name="currentBankBalances">The bank balances as at the reconciliation date to include in this new single line of the ledger book.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="budgetCollection" /> is in an invalid state, ie the supplied budget is not active.</exception>
        /// <exception cref="ValidationWarningException">
        ///     Thrown when there are outstanding validation errors in the <paramref name="statement" />, <paramref name="budgetCollection" />, or <paramref name="ledgerBook" />.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the supplied dates are invalid or not consistent with the<paramref name="ledgerBook" />.</exception>
        ReconciliationResult PeriodEndReconciliation([NotNull] LedgerBook ledgerBook,
                                                     DateTime closingDateExclusive,
                                                     [NotNull] BudgetCollection budgetCollection,
                                                     [NotNull] StatementModel statement,
                                                     bool ignoreWarnings,
                                                     [NotNull] params BankBalance[] currentBankBalances);

        /// <summary>
        ///     Performs a funds transfer for the given ledger entry line to transfer funds from one ledger bucket to another.
        /// </summary>
        void TransferFunds([NotNull] LedgerBook ledgerBook, [NotNull] TransferFundsCommand transferDetails, [NotNull] LedgerEntryLine ledgerEntryLine);

        /// <summary>
        ///     Examines the ledger book's most recent reconciliation looking for transactions waiting to be matched to transactions imported in the current month.
        ///     If any transactions are found, the statement is then examined to see if the transactions appear, if they do not a new <see cref="ValidationWarningException" />
        ///     is thrown; otherwise the method returns.
        /// </summary>
        void ValidateAgainstOrphanedAutoMatchingTransactions([NotNull] LedgerBook ledgerBook, [NotNull] StatementModel statement);
    }
}
