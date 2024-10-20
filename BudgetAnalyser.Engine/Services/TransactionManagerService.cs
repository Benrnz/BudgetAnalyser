using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to manipulate and manage transactions and statements.
    /// </summary>
    /// <seealso cref="ITransactionManagerService" />
    /// <seealso cref="ISupportsModelPersistence" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class TransactionManagerService : ITransactionManagerService, ISupportsModelPersistence
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;
        private readonly MonitorableDependencies monitorableDependencies;
        private readonly IStatementRepository statementRepository;
        private BudgetCollection budgetCollection;
        private int budgetHash;
        private bool sortedByBucket;
        private ObservableCollection<Transaction> transactions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionManagerService" /> class.
        /// </summary>
        /// <param name="bucketRepository">The bucket repository.</param>
        /// <param name="statementRepository">The statement repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="monitorableDependencies">The dependency monitor manager</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public TransactionManagerService(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IStatementRepository statementRepository,
            [NotNull] ILogger logger,
            [NotNull] MonitorableDependencies monitorableDependencies)
        {
            if (bucketRepository is null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (statementRepository is null)
            {
                throw new ArgumentNullException(nameof(statementRepository));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (monitorableDependencies is null) throw new ArgumentNullException(nameof(monitorableDependencies));

            this.bucketRepository = bucketRepository;
            this.statementRepository = statementRepository;
            this.logger = logger;
            this.monitorableDependencies = monitorableDependencies;
        }

        /// <summary>
        ///     Occurs when the underlying storage for transactions is closed.
        ///     This allows the UI to update and clear accordingly.
        ///     Opening and closing files is controlled centrally, not by this service.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        ///     Occurs when a new data source has been loaded and is now available for use.
        /// </summary>
        public event EventHandler NewDataSourceAvailable;

        /// <summary>
        ///     Occurs when the service has finished saving data. This allows the controller to update any clientside view-models.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        ///     Occurs just before Saving the model. Can be used to request more information from the UI Controllers.
        /// </summary>
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;

        /// <summary>
        ///     Occurs just before Validating the model.  Can be used to ensure the UI Controller has updated any necessary
        ///     information with its service.
        /// </summary>
        public event EventHandler<ValidatingEventArgs> Validating;

        /// <summary>
        ///     Gets the calculated average debit.
        /// </summary>
        public decimal AverageDebit
        {
            get
            {
                if (this.transactions is null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount < 0).SafeAverage(t => t.Amount);
            }
        }

        /// <summary>
        ///     Gets the type of the data the implementation deals with.
        /// </summary>
        public ApplicationDataType DataType => ApplicationDataType.Transactions;

        /// <summary>
        ///     Gets the initialisation sequence number. Set this to a low number for important data that needs to be loaded first.
        /// </summary>
        public int LoadSequence => 10;

        /// <summary>
        ///     Gets the statement model.
        /// </summary>
        public StatementModel StatementModel { get; private set; }

        /// <summary>
        ///     Gets the calculated total count.
        /// </summary>
        public decimal TotalCount
        {
            get
            {
                if (this.transactions is null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Count();
            }
        }

        /// <summary>
        ///     Gets the calculated total credits.
        /// </summary>
        public decimal TotalCredits
        {
            get
            {
                if (this.transactions is null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            }
        }

        /// <summary>
        ///     Gets the calculated total debits.
        /// </summary>
        public decimal TotalDebits
        {
            get
            {
                if (this.transactions is null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            }
        }

        /// <summary>
        ///     Closes the currently loaded file.  No warnings will be raised if there is unsaved data.
        /// </summary>
        public void Close()
        {
            this.transactions = new ObservableCollection<Transaction>();
            StatementModel?.Dispose();
            StatementModel = null;
            this.budgetCollection = null;
            this.budgetHash = 0;
            var handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Create a new <see cref="StatementModel" />.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.StatementModelStorageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            await this.statementRepository.CreateNewAndSaveAsync(applicationDatabase.StatementModelStorageKey);
            await LoadAsync(applicationDatabase);
        }

        /// <summary>
        ///     Loads a data source with the provided database reference data asynchronously.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="DataFormatException">Statement Model data is corrupt and has been tampered with. Unable to load.</exception>
        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase is null)
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            StatementModel?.Dispose();
            try
            {
                StatementModel = await this.statementRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.StatementModelStorageKey), applicationDatabase.IsEncrypted);
            }
            catch (StatementModelChecksumException ex)
            {
                throw new DataFormatException("Statement Model data is corrupt and has been tampered with. Unable to load.", ex);
            }

            NewDataAvailable();
        }

        /// <summary>
        ///     Saves the application database asynchronously. This may be called using a background worker thread.
        /// </summary>
        /// <exception cref="ValidationWarningException">
        ///     Unable to save transactions at this time, some data is invalid.  +
        ///     messages
        /// </exception>
        public async Task SaveAsync(ApplicationDatabase applicationDatabase)
        {
            if (StatementModel is null)
            {
                return;
            }

            EventHandler<AdditionalInformationRequestedEventArgs> handler = Saving;
            handler?.Invoke(this, new AdditionalInformationRequestedEventArgs());

            var messages = new StringBuilder();
            if (!ValidateModel(messages))
            {
                throw new ValidationWarningException("Unable to save transactions at this time, some data is invalid. " + messages);
            }

            StatementModel.StorageKey = applicationDatabase.FullPath(applicationDatabase.StatementModelStorageKey);
            await this.statementRepository.SaveAsync(StatementModel, applicationDatabase.IsEncrypted);
            this.monitorableDependencies.NotifyOfDependencyChange(StatementModel);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called before Save is called. This will be called on the UI Thread.
        ///     Objects can optionally add some context data that will be passed to the <see cref="SaveAsync" /> method call.
        ///     This can be used to finalise any edits or prompt the user for closing data, ie, a "what-did-you-change" comment;
        ///     this
        ///     can't be done during save as it may not be called using the UI Thread.
        /// </summary>
        public void SavePreview()
        {
        }

        /// <summary>
        ///     Validates the model owned by the service.
        /// </summary>
        public bool ValidateModel(StringBuilder messages)
        {
            Validating?.Invoke(this, new ValidatingEventArgs());

            // In the case of the StatementModel all edits are validated and resolved during data edits. No need for an overall consistency check.
            return true;
        }

        /// <summary>
        ///     Clears the bucket and text filters.
        /// </summary>
        public ObservableCollection<Transaction> ClearBucketAndTextFilters()
        {
            ResetTransactionsCollection();
            return this.transactions;
        }

        /// <summary>
        ///     Detects duplicate transactions in the current <see cref="StatementModel" /> and returns a summary string for
        ///     displaying in the UI. Each individual duplicate transactions will be flagged by the
        ///     <see cref="Transaction.IsSuspectedDuplicate" /> property.
        /// </summary>
        /// <returns>
        ///     A textual summary of duplicates found. Null if none are detected or no statement is loaded.
        /// </returns>
        public string DetectDuplicateTransactions()
        {
            if (StatementModel is null)
            {
                return null;
            }

            List<IGrouping<int, Transaction>> duplicates = StatementModel.ValidateAgainstDuplicates().ToList();
            return duplicates.Any()
                ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!",
                    duplicates.Sum(group => group.Count()))
                : null;
        }

        /// <summary>
        ///     Provides a list of buckets for display purposes for filtering the transactions shown. This list will include a
        ///     blank item to represent no filtering, and a [Uncategorised] to represent a filter to show only transactions with no
        ///     bucket allocation.
        /// </summary>
        /// <returns>
        ///     A string list of bucket codes.
        /// </returns>
        public IEnumerable<string> FilterableBuckets()
        {
            return this.bucketRepository.Buckets
                .Where(b => b.Active)
                .Select(b => b.Code)
                .Union(new[] { string.Empty, TransactionConstants.UncategorisedFilter })
                .OrderBy(b => b);
        }

        /// <summary>
        ///     Returns a filtered list of <see cref="Transaction" />s by bucket code.
        /// </summary>
        /// <param name="bucketCode">
        ///     The bucket code as text. This can be null or return all, and
        ///     <see cref="TransactionConstants.UncategorisedFilter" /> to
        ///     only return transactions without a bucket classification.
        /// </param>
        public ObservableCollection<Transaction> FilterByBucket(string bucketCode)
        {
            if (bucketCode == TransactionConstants.UncategorisedFilter)
            {
                return this.transactions = new ObservableCollection<Transaction>(StatementModel.Transactions.Where(t => t.BudgetBucket is null));
            }

            var bucket = bucketCode is null ? null : this.bucketRepository.GetByCode(bucketCode);

            if (bucket is null)
            {
                return new ObservableCollection<Transaction>(StatementModel.Transactions);
            }

            var paternityTest = new BudgetBucketPaternity();
            return this.transactions = new ObservableCollection<Transaction>(StatementModel.Transactions.Where(t => paternityTest.OfSameBucketFamily(t.BudgetBucket, bucket)));
        }

        /// <summary>
        ///     Returns a filtered list of <see cref="Transaction" />s using the provided search text.  All following transaction
        ///     fields are searched: Description, Reference1, Reference2, Reference3.
        /// </summary>
        /// <param name="searchText">The search text. Minimum 3 characters. A Null value clears the search.</param>
        public ObservableCollection<Transaction> FilterBySearchText(string? searchText)
        {
            if (searchText.IsNothing())
            {
                return ClearBucketAndTextFilters();
            }

            if (searchText.Length < 3)
            {
                return ClearBucketAndTextFilters();
            }

            this.transactions = new ObservableCollection<Transaction>(
                StatementModel.Transactions.Where(t => MatchTransactionText(t, searchText))
                    .AsParallel()
                    .ToList());
            return this.transactions;
        }

        /// <summary>
        ///     Filters the transactions using the filter object provided.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria is null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            this.monitorableDependencies.NotifyOfDependencyChange(criteria);
            StatementModel.Filter(criteria);
        }

        /// <summary>
        ///     Imports a bank's transaction extract and merges it with the currently loaded Budget Analyser Statement.
        ///     This method should not be used without a <see cref="StatementModel" /> loaded.
        ///     It is recommended to follow this up with <see cref="ValidateWithCurrentBudgetsAsync" />.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     There are no transactions loaded, you must first load an existing
        ///     file or create a new one.
        /// </exception>
        /// <exception cref="BudgetAnalyser.Engine.Statement.TransactionsAlreadyImportedException"></exception>
        public async Task ImportAndMergeBankStatementAsync(string storageKey, Account account)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (StatementModel is null)
            {
                throw new InvalidOperationException(
                    "There are no transactions loaded, you must first load an existing file or create a new one.");
            }

            var additionalModel = await this.statementRepository.ImportBankStatementAsync(storageKey, account);
            var combinedModel = StatementModel.Merge(additionalModel);
            var  minDate = additionalModel.AllTransactions.Min(t => t.Date);
            var maxDate = additionalModel.AllTransactions.Max(t => t.Date);
            IEnumerable<IGrouping<int, Transaction>> duplicates = combinedModel.ValidateAgainstDuplicates(minDate, maxDate).ToList();
            if (duplicates.Count() == additionalModel.AllTransactions.Count())
            {
                throw new TransactionsAlreadyImportedException();
            }

            StatementModel.Dispose();
            StatementModel = combinedModel;
            NewDataAvailable();
        }

        /// <summary>
        ///     Parses and loads the persisted state data from the provided object.
        /// </summary>
        /// <param name="stateData">The state data loaded from persistent storage.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Initialise(StatementApplicationState stateData)
        {
            if (stateData is null)
            {
                throw new ArgumentNullException(nameof(stateData));
            }

            this.budgetHash = 0;
            this.sortedByBucket = stateData.SortByBucket ?? false;
        }

        /// <summary>
        ///     Prepares the persistent state data to save to storage.
        /// </summary>
        public StatementApplicationState PreparePersistentStateData()
        {
            return new StatementApplicationState
            {
                SortByBucket = this.sortedByBucket
            };
        }

        /// <summary>
        ///     Removes the provided transaction from the currently loaded Budget Analyser Statement.
        /// </summary>
        /// <param name="transactionToRemove">The transaction to remove.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void RemoveTransaction(Transaction transactionToRemove)
        {
            if (transactionToRemove is null)
            {
                throw new ArgumentNullException(nameof(transactionToRemove));
            }

            StatementModel.RemoveTransaction(transactionToRemove);
        }

        /// <summary>
        ///     Splits the provided transaction into two. The provided transactions is removed, and two new transactions are
        ///     created. Both transactions must add up to the existing transaction amount.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2,
                                     BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
        {
            if (originalTransaction is null)
            {
                throw new ArgumentNullException(nameof(originalTransaction));
            }

            if (splinterBucket1 is null)
            {
                throw new ArgumentNullException(nameof(splinterBucket1));
            }

            if (splinterBucket2 is null)
            {
                throw new ArgumentNullException(nameof(splinterBucket2));
            }

            StatementModel.SplitTransaction(
                originalTransaction,
                splinterAmount1,
                splinterAmount2,
                splinterBucket1,
                splinterBucket2);
        }

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
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task<bool> ValidateWithCurrentBudgetsAsync(BudgetCollection budgets = null)
        {
            // This method must be called at least once with a budget collection.  Second and subsequent times do not require the budget.
            if (this.budgetCollection is null && budgets is null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            this.budgetCollection = budgets ?? this.budgetCollection;

            if (StatementModel is null)
            {
                // Can't check yet, statement hasn't been loaded yet. Everything is ok for now.
                return true;
            }

            if (this.budgetCollection.GetHashCode() == this.budgetHash)
            {
                // This budget has already been checked against this statement. No need to repeatedly check the validity below, this is an expensive operation.
                // Everything is ok.
                return true;
            }

            var allBuckets = new List<BudgetBucket>(this.bucketRepository.Buckets.OrderBy(b => b.Code));
            var allTransactionHaveABucket = await Task.Run(
                () =>
                {
                    return StatementModel.AllTransactions
                        .Where(t => t.BudgetBucket is not null)
                        .AsParallel()
                        .All(
                            t =>
                            {
                                var bucketExists = allBuckets.Contains(t.BudgetBucket);
                                if (!bucketExists)
                                {
                                    t.BudgetBucket = null;
                                    this.logger.LogWarning(l => l.Format("Transaction {0} has a bucket ({1}) that doesn't exist!", t.Date, t.BudgetBucket));
                                }
                                return bucketExists;
                            });
                });

            this.budgetHash = this.budgetCollection.GetHashCode();
            return allTransactionHaveABucket;
        }

        private static bool MatchTransactionText(Transaction t, string textFilter)
        {
            if (!string.IsNullOrWhiteSpace(t.Description))
            {
                if (t.Description.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference1))
            {
                if (t.Reference1.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference2))
            {
                if (t.Reference2.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(t.Reference3))
            {
                if (t.Reference3.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void NewDataAvailable()
        {
            ResetTransactionsCollection();
            this.monitorableDependencies.NotifyOfDependencyChange(StatementModel);
            NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
        }

        private void ResetTransactionsCollection()
        {
            this.transactions = StatementModel is null
                ? new ObservableCollection<Transaction>()
                : new ObservableCollection<Transaction>(StatementModel.Transactions);
        }
    }
}