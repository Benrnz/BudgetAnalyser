using System.Collections.Generic;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to provide access to manipulate ledger books.
    ///     This service is designed to be stateful.
    /// </summary>
    public interface ILedgerService : INotifyDatabaseChanges, IServiceFoundation
    {
        /// <summary>
        ///     Gets the ledger book model.
        /// </summary>
        LedgerBook LedgerBook { get; }

        /// <summary>
        ///     Moves the specified ledger to the specified account.
        /// </summary>
        /// <param name="ledger">The ledger column to move.</param>
        /// <param name="storedInAccount">The new account to store the ledger in.</param>
        void MoveLedgerToAccount([NotNull] LedgerBucket ledger, [NotNull] Account storedInAccount);

        /// <summary>
        ///     Renames the ledger book.
        /// </summary>
        /// <param name="newName">The new name.</param>
        void RenameLedgerBook([NotNull] string newName);

        /// <summary>
        ///     Tracks a new budget bucket by creating a new <see cref="LedgerBucket" /> for the given <see cref="BudgetBucket" />
        ///     and adds the ledger to the ledger book.
        /// </summary>
        /// <param name="bucket">The bucket to track.</param>
        /// <param name="storeInThisAccount">The account to store the ledger's funds.</param>
        LedgerBucket TrackNewBudgetBucket([NotNull] ExpenseBucket bucket, [NotNull] Account storeInThisAccount);

        /// <summary>
        ///     Returns a list of valid accounts for use with the Ledger Book.
        /// </summary>
        IEnumerable<Account> ValidLedgerAccounts();
    }
}