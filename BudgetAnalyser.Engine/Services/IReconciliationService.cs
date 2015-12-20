using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to control the reconciliation process from the UI.
    /// </summary>
    public interface IReconciliationService
    {
        /// <summary>
        ///     Gets the user reminder task list for reconciliation.
        /// </summary>
        ToDoCollection ReconciliationToDoList { get; }

        /// <summary>
        ///     An optional validation method the UI can call before invoking <see cref="MonthEndReconciliation" /> to test for
        ///     validation warnings.
        ///     If validation fails a new <see cref="ValidationWarningException" /> is thrown; otherwise the method returns.
        /// </summary>
        void BeforeReconciliationValidation(LedgerBook book, StatementModel model);

        /// <summary>
        ///     Cancels an existing balance adjustment transaction that already exists in the Ledger Entry Line.
        /// </summary>
        void CancelBalanceAdjustment([NotNull] LedgerEntryLine entryLine, Guid transactionId);

        /// <summary>
        ///     Creates a new balance adjustment transaction for the given entry line.
        /// </summary>
        LedgerTransaction CreateBalanceAdjustment([NotNull] LedgerEntryLine entryLine, decimal amount,
            [NotNull] string narrative, [NotNull] Account account);

        /// <summary>
        ///     Creates a new ledger transaction in the given Ledger. The Ledger Entry must exist in the current Ledger Book.
        /// </summary>
        LedgerTransaction CreateLedgerTransaction([NotNull] LedgerEntryLine reconciliation,
            [NotNull] LedgerEntry ledgerEntry, decimal amount, [NotNull] string narrative);

        /// <summary>
        ///     Creates a new LedgerEntryLine for the specified <see cref="LedgerBook" /> to begin reconciliation.
        /// </summary>
        /// <param name="ledgerBook">The ledger book to to which this new reconciliation applies.</param>
        /// <param name="reconciliationDate">
        ///     The startDate for the <see cref="LedgerEntryLine" />. This is usually the previous Month's "Reconciliation-Date",
        ///     as this month's reconciliation starts with this date and includes transactions
        ///     from that date. This date is different to the "Reconciliation-Date" that appears next to the resulting
        ///     reconciliation which is the end date for the period.
        /// </param>
        /// <param name="budgetContext">The current budget context.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <param name="balances">
        ///     The bank balances as at the <paramref name="reconciliationDate" /> to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook" /> is in an invalid state.</exception>
        LedgerEntryLine MonthEndReconciliation(
            [NotNull] LedgerBook ledgerBook,
            DateTime reconciliationDate,
            [NotNull] IBudgetCurrencyContext budgetContext,
            [NotNull] StatementModel statement,
            bool ignoreWarnings,
            [NotNull] params BankBalance[] balances);

        /// <summary>
        ///     Removes the transaction from the specified Ledger Entry.
        /// </summary>
        void RemoveTransaction([NotNull] LedgerEntry ledgerEntry, Guid transactionId);

        /// <summary>
        ///     Transfer funds from one ledger bucket to another. This is only possible if the current ledger reconciliation is
        ///     unlocked.
        ///     This is usually used during reconciliation.
        /// </summary>
        /// <param name="reconciliation">
        ///     The reconciliation line that this transfer will be created in.  A transfer can only occur
        ///     between two ledgers in the same reconciliation.
        /// </param>
        /// <param name="transferDetails">The details of the requested transfer.</param>
        void TransferFunds([NotNull] LedgerEntryLine reconciliation, [NotNull] TransferFundsCommand transferDetails);

        /// <summary>
        ///     Unlocks the current month after it has been saved and locked.
        /// </summary>
        LedgerEntryLine UnlockCurrentMonth([NotNull] LedgerBook ledgerBook);

        /// <summary>
        ///     Updates the remarks for the given Ledger Entry Line.
        /// </summary>
        void UpdateRemarks([NotNull] LedgerEntryLine entryLine, [NotNull] string remarks);
    }
}