using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    public interface ITransactionManagerService : IServiceFoundation
    {
        StatementModel StatementModel { get; }
        Task<bool> CheckBudgetContainsAllUsedBucketsInStatementAsync(BudgetCollection budgets = null);
        string DetectDuplicateTransactions();
        IEnumerable<string> FilterableBuckets();
        void FilterTransactions([NotNull] GlobalFilterCriteria criteria);
        void FilterTransactions([NotNull] string searchText);

        /// <summary>
        ///     Imports a bank's transaction extract and returns it as a new <see cref="StatementModel" />.  This can then be
        ///     merged
        ///     with another <see cref="StatementModel" /> using the
        ///     <see cref="Statement.StatementModel.Merge(BudgetAnalyser.Engine.Statement.StatementModel)" /> method.
        /// </summary>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given
        ///     <paramref name="storageKey" />
        /// </exception>
        StatementModel ImportAndMergeBankStatementAsync(
            [NotNull] string storageKey,
            [NotNull] AccountType account);

        StatementApplicationState LoadPersistedStateData(object stateData);

        /// <summary>
        ///     Loads an existing Budget Analyser <see cref="StatementModel" />.
        /// </summary>
        /// <param name="storageKey">Pass a known storage key (database identifier or filename) to load.</param>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given
        ///     <paramref name="storageKey" />
        /// </exception>
        /// <exception cref="StatementModelChecksumException">
        ///     Will be thrown if the statement model's internal checksum detects
        ///     corrupt data indicating tampering.
        /// </exception>
        /// <exception cref="DataFormatException">
        ///     Will be thrown if the format of the bank extract contains unexpected data
        ///     indicating it is corrupt or an old file.
        /// </exception>
        Task<StatementModel> LoadStatementModelAsync([NotNull] string storageKey);

        void Merge([NotNull] StatementModel additionalModel);
        object PreparePersistentStateData();
        void RemoveTransaction([NotNull] Transaction transactionToRemove);

        /// <summary>
        ///     Save the given <see cref="StatementModel" /> into persistent storage.
        ///     (Saving and preserving bank statement files is not supported.)
        /// </summary>
        Task SaveAsync();

        void SplitTransaction(
            [NotNull] Transaction originalTransaction,
            decimal splinterAmount1,
            decimal splinterAmount2,
            [NotNull] BudgetBucket splinterBucket1,
            [NotNull] BudgetBucket splinterBucket2);
    }

    [AutoRegisterWithIoC]
    public class TransactionManagerService : ITransactionManagerService
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;
        private readonly IStatementRepository statementRepository;
        private BudgetCollection budgetCollection;
        private int budgetHash;
        private string currentStorageKey;
        private bool sortedByBucket;

        public TransactionManagerService([NotNull] IBudgetBucketRepository bucketRepository, [NotNull] IStatementRepository statementRepository, [NotNull] ILogger logger)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (statementRepository == null)
            {
                throw new ArgumentNullException("statementRepository");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.bucketRepository = bucketRepository;
            this.statementRepository = statementRepository;
            this.logger = logger;
        }

        public StatementModel StatementModel { get; private set; }

        public async Task<bool> CheckBudgetContainsAllUsedBucketsInStatementAsync(BudgetCollection budgets = null)
        {
            // This method must be called at least once with a budget collection.  Second and subsequent times do not require the budget.
            if (this.budgetCollection == null && budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            this.budgetCollection = budgets ?? this.budgetCollection;

            if (StatementModel == null)
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
                        .AsParallel()
                        .All(
                            t =>
                            {
                                var bucketExists = allBuckets.Contains(t.BudgetBucket);
                                if (!bucketExists)
                                {
                                    this.logger.LogWarning(l => l.Format("Transaction {0} has a bucket ({1}) that doesn't exist!", t.Date, t.BudgetBucket));
                                }
                                return bucketExists;
                            });
                });

            this.budgetHash = this.budgetCollection.GetHashCode();
            return allTransactionHaveABucket;
        }

        public string DetectDuplicateTransactions()
        {
            var duplicates = StatementModel.ValidateAgainstDuplicates().ToList();
            return duplicates.Any()
                ? String.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!", duplicates.Sum(group => group.Count()))
                : null;
        }

        public IEnumerable<string> FilterableBuckets()
        {
            return this.bucketRepository.Buckets
                .Select(b => b.Code)
                .Union(new[] { String.Empty, UncategorisedFilter })
                .OrderBy(b => b);
        }

        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            StatementModel.Filter(criteria);
        }

        public void FilterTransactions(string searchText)
        {
            if (String.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentNullException("searchText");
            }

            StatementModel.FilterByText(searchText);
        }

        public StatementModel ImportAndMergeBankStatementAsync(string storageKey, AccountType account)
        {
            return this.statementRepository.ImportAndMergeBankStatementAsync(storageKey, account);
        }

        public StatementApplicationState LoadPersistedStateData(object stateData)
        {
            this.budgetHash = 0;
            var state = (StatementApplicationState)stateData;
            this.sortedByBucket = state.SortByBucket ?? false;
            this.currentStorageKey = state.StorageKey;
            return state;
        }

        /// <summary>
        ///     Loads an existing Budget Analyser <see cref="ITransactionManagerService.StatementModel" />.
        /// </summary>
        /// <param name="storageKey">Pass a known storage key (database identifier or filename) to load.</param>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given
        ///     <paramref name="storageKey" />
        /// </exception>
        /// <exception cref="StatementModelChecksumException">
        ///     Will be thrown if the statement model's internal checksum detects
        ///     corrupt data indicating tampering.
        /// </exception>
        /// <exception cref="DataFormatException">
        ///     Will be thrown if the format of the bank extract contains unexpected data
        ///     indicating it is corrupt or an old file.
        /// </exception>
        public async Task<StatementModel> LoadStatementModelAsync(string storageKey)
        {
            StatementModel = await this.statementRepository.LoadStatementModelAsync(storageKey);
            return StatementModel;
        }

        public void Merge(StatementModel additionalModel)
        {
            if (additionalModel == null)
            {
                throw new ArgumentNullException("additionalModel");
            }

            StatementModel.Merge(additionalModel);
        }

        public object PreparePersistentStateData()
        {
            throw new NotImplementedException();
            //return new StatementApplicationState()
            //{
            //    StorageKey = StatementModel.StorageKey,
            //    SortByBucket = 

            //}
        }

        public void RemoveTransaction(Transaction transactionToRemove)
        {
            if (transactionToRemove == null)
            {
                throw new ArgumentNullException("transactionToRemove");
            }

            StatementModel.RemoveTransaction(transactionToRemove);
        }

        public async Task SaveAsync()
        {
            await this.statementRepository.SaveAsync(StatementModel);
        }

        public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
        {
            if (originalTransaction == null)
            {
                throw new ArgumentNullException("originalTransaction");
            }

            if (splinterBucket1 == null)
            {
                throw new ArgumentNullException("splinterBucket1");
            }

            if (splinterBucket2 == null)
            {
                throw new ArgumentNullException("splinterBucket2");
            }

            StatementModel.SplitTransaction(
                originalTransaction,
                splinterAmount1,
                splinterAmount2,
                splinterBucket1,
                splinterBucket2);
        }
    }
}