using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to provide access to manipulate ledger books.
    /// </summary>
    public interface ILedgerService : IServiceFoundation
    {
        /// <summary>
        ///     Creates a new empty <see cref="LedgerBook" />.
        /// </summary>
        /// <param name="storageKey">A new unique identifier for a ledger book. Will be overwritten if it exists.</param>
        LedgerBook CreateNew([NotNull] string storageKey);

        /// <summary>
        ///     Retrieves and returns the ledger book from the specified file to display in the UI.
        /// </summary>
        /// <param name="storageKey">The unique identifier of the Ledger Book.</param>
        LedgerBook DisplayLedgerBook([NotNull] string storageKey);

        /// <summary>
        ///     Creates a new LedgerEntryLine for the specified <see cref="LedgerBook" /> to begin reconciliation.
        /// </summary>
        /// <param name="date">
        ///     The date for the <see cref="LedgerEntryLine" />. Also used to search for transactions in the
        ///     <see cref="statement" />. This date ideally is your payday of the month, and should be the same date
        ///     every month. Transactions are searched for up to but not including this date.
        /// </param>
        /// <param name="balances">
        ///     The bank balances as at the <see cref="date" /> to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budgetContext">The current budget context.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook" /> is in an invalid state.</exception>
        LedgerEntryLine MonthEndReconciliation(
            DateTime date,
            [NotNull] IEnumerable<BankBalance> balances,
            [NotNull] IBudgetCurrencyContext budgetContext,
            [NotNull] StatementModel statement,
            bool ignoreWarnings = false);

        /// <summary>
        ///     Moves the specified ledger to the specified account.
        /// </summary>
        /// <param name="ledgerBook">The ledger book.</param>
        /// <param name="ledger">The ledger column to move.</param>
        /// <param name="storedInAccount">The new account to store the ledger in.</param>
        void MoveLedgerToAccount([NotNull] LedgerBook ledgerBook, [NotNull] LedgerColumn ledger, [NotNull] AccountType storedInAccount);

        /// <summary>
        ///     Removes the most recent reconciliation <see cref="LedgerEntryLine" />.
        /// </summary>
        /// <param name="line">The line.</param>
        void RemoveReconciliation([NotNull] LedgerEntryLine line);

        /// <summary>
        ///     Renames the ledger book.
        /// </summary>
        /// <param name="ledgerBook">The ledger book.</param>
        /// <param name="newName">The new name.</param>
        void RenameLedgerBook([NotNull] LedgerBook ledgerBook, [NotNull] string newName);

        /// <summary>
        ///     Saves the specified ledger book.
        /// </summary>
        /// <param name="ledgerBook">The ledger book.</param>
        /// <param name="storageKey">The unique identifier of the Ledger Book.</param>
        void Save([NotNull] LedgerBook ledgerBook, string storageKey = null);

        /// <summary>
        ///     Tracks a new budget bucket by creating a new <see cref="LedgerColumn" /> for the given <see cref="BudgetBucket" />
        ///     and adds the ledger to the ledger book.
        /// </summary>
        /// <param name="bucket">The bucket to track.</param>
        /// <param name="storeInThisAccount">The account to store the ledger's funds.</param>
        /// <returns></returns>
        LedgerColumn TrackNewBudgetBucket([NotNull] ExpenseBucket bucket, [NotNull] AccountType storeInThisAccount);

        /// <summary>
        ///     Unlocks the current month after it has been saved and locked.
        /// </summary>
        LedgerEntryLine UnlockCurrentMonth();
    }
}