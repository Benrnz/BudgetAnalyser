using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An interface for managing, viewing, and storing transactions
    ///     This service is designed to be stateful.
    /// </summary>
    public interface ITransactionManagerService : INotifyDatabaseChanges, IServiceFoundation
    {
        /// <summary>
        ///     Gets the calculated average debit.
        /// </summary>
        decimal AverageDebit { get; }

        /// <summary>
        ///     Gets the statement model.
        /// </summary>
        StatementModel StatementModel { get; }

        /// <summary>
        ///     Gets the calculated total count.
        /// </summary>
        decimal TotalCount { get; }

        /// <summary>
        ///     Gets the calculated total credits.
        /// </summary>
        decimal TotalCredits { get; }

        /// <summary>
        ///     Gets calculated the total debits.
        /// </summary>
        decimal TotalDebits { get; }

        /// <summary>
        ///     Clears the bucket and text filters.
        /// </summary>
        ObservableCollection<Transaction> ClearBucketAndTextFilters();

        /// <summary>
        ///     Detects duplicate transactions in the current <see cref="StatementModel" /> and returns a summary string for
        ///     displaying in the UI.
        ///     Each individual duplicate transactions will be flagged by the <see cref="Transaction.IsSuspectedDuplicate" />
        ///     property.
        /// </summary>
        /// <returns>A textual summary of duplicates found. Null if none are detected or no statement is loaded.</returns>
        string DetectDuplicateTransactions();

        /// <summary>
        ///     Provides a list of buckets for display purposes for filtering the transactions shown.
        ///     This list will include a blank item to represent no filtering, and a [Uncategorised] to represent a filter to show
        ///     only transactions
        ///     with no bucket allocation.
        /// </summary>
        /// <returns>A string list of bucket codes.</returns>
        IEnumerable<string> FilterableBuckets();

        /// <summary>
        ///     Returns a filtered list of <see cref="Transaction" />s by bucket code.
        /// </summary>
        /// <param name="bucketCode">
        ///     The bucket code as text. This can be null or return all, and
        ///     <see cref="TransactionConstants.UncategorisedFilter" /> to
        ///     only return transactions without a bucket classification.
        /// </param>
        ObservableCollection<Transaction> FilterByBucket([CanBeNull] string bucketCode);

        /// <summary>
        ///     Returns a filtered list of <see cref="Transaction" />s using the provided search text.  All following transaction
        ///     fields are searched: Description, Reference1, Reference2, Reference3.
        /// </summary>
        /// <param name="searchText">The search text. Minimum 3 characters.</param>
        ObservableCollection<Transaction> FilterBySearchText(string searchText);

        /// <summary>
        ///     Filters the transactions using the filter object provided.
        /// </summary>
        void FilterTransactions([NotNull] GlobalFilterCriteria criteria);

        /// <summary>
        ///     Imports a bank's transaction extract and merges it with the currently loaded Budget Analyser Statement.
        ///     This method should not be used without a <see cref="StatementModel" /> loaded.
        ///     It is recommended to follow this up with <see cref="ValidateWithCurrentBudgetsAsync" />.
        /// </summary>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given
        ///     <paramref name="storageKey" />
        /// </exception>
        /// <exception cref="TransactionsAlreadyImportedException">Will be thrown if the supplied file has already been imported.</exception>
        Task ImportAndMergeBankStatementAsync(
            [NotNull] string storageKey,
            [NotNull] Account account);

        /// <summary>
        ///     Parses and loads the persisted state data from the provided object.
        /// </summary>
        /// <param name="stateData">The state data loaded from persistent storage.</param>
        void Initialise([NotNull] StatementApplicationState stateData);

        /// <summary>
        ///     Prepares the persistent state data to save to storage.
        /// </summary>
        StatementApplicationState PreparePersistentStateData();

        /// <summary>
        ///     Removes the provided transaction from the currently loaded Budget Analyser Statement.
        /// </summary>
        /// <param name="transactionToRemove">The transaction to remove.</param>
        void RemoveTransaction([NotNull] Transaction transactionToRemove);

        /// <summary>
        ///     Splits the provided transaction into two. The provided transactions is removed, and two new transactions are
        ///     created.
        ///     Both transactions must add up to the existing transaction amount.
        /// </summary>
        /// <param name="originalTransaction">The original transaction.</param>
        /// <param name="splinterAmount1">The splinter amount1.</param>
        /// <param name="splinterAmount2">The splinter amount2.</param>
        /// <param name="splinterBucket1">The splinter bucket1.</param>
        /// <param name="splinterBucket2">The splinter bucket2.</param>
        void SplitTransaction(
            [NotNull] Transaction originalTransaction,
            decimal splinterAmount1,
            decimal splinterAmount2,
            [NotNull] BudgetBucket splinterBucket1,
            [NotNull] BudgetBucket splinterBucket2);

        /// <summary>
        ///     Validates the currently loaded <see cref="StatementModel" /> against the provided budgets and ensures all buckets
        ///     used by the transactions
        ///     exist in the budgets.  This is performed asynchronously.
        ///     This method can be called when a budget is loaded or changed or when a new Budget Analyser Statement is loaded.
        /// </summary>
        /// <param name="budgets">
        ///     The current budgets. This must be provided at least once. It can be omitted when
        ///     calling this method after the statement model has changed if the budget was previously provided.
        /// </param>
        /// <returns>
        ///     A task that will result in true if all buckets used, are present in the budgets, otherwise false.
        ///     If false, this indicates that some transactions may have their bucket allocation removed possibly resulting in
        ///     unintended data loss.
        /// </returns>
        Task<bool> ValidateWithCurrentBudgetsAsync(BudgetCollection budgets = null);
    }
}