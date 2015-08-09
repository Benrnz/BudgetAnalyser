using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class TransactionManagerService : ITransactionManagerService, ISupportsModelPersistence
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;
        private readonly IStatementRepository statementRepository;
        private BudgetCollection budgetCollection;
        private int budgetHash;
        private bool sortedByBucket;
        private ObservableCollection<Transaction> transactions;

        public TransactionManagerService(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IStatementRepository statementRepository,
            [NotNull] ILogger logger)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (statementRepository == null)
            {
                throw new ArgumentNullException(nameof(statementRepository));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.bucketRepository = bucketRepository;
            this.statementRepository = statementRepository;
            this.logger = logger;
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;

        public decimal AverageDebit
        {
            get
            {
                if (this.transactions == null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount < 0).Average(t => t.Amount);
            }
        }

        public  ApplicationDataType DataType => ApplicationDataType.Transactions;

        public  int LoadSequence => 10;

        public StatementModel StatementModel { get; private set; }

        public decimal TotalCount
        {
            get
            {
                if (this.transactions == null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Count();
            }
        }

        public decimal TotalCredits
        {
            get
            {
                if (this.transactions == null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            }
        }

        public decimal TotalDebits
        {
            get
            {
                if (this.transactions == null || this.transactions.None())
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            }
        }

        public ObservableCollection<Transaction> ClearBucketAndTextFilters()
        {
            ResetTransactionsCollection();
            return this.transactions;
        }

        public void Close()
        {
            this.transactions = new ObservableCollection<Transaction>();
            StatementModel = null;
            this.budgetCollection = null;
            this.budgetHash = 0;
            EventHandler handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.StatementModelStorageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            await this.statementRepository.CreateNewAndSaveAsync(applicationDatabase.StatementModelStorageKey);
            await LoadAsync(applicationDatabase);
        }

        public string DetectDuplicateTransactions()
        {
            if (StatementModel == null)
            {
                return null;
            }

            List<IGrouping<int, Transaction>> duplicates = StatementModel.ValidateAgainstDuplicates().ToList();
            return duplicates.Any()
                ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!", duplicates.Sum(group => group.Count()))
                : null;
        }

        public IEnumerable<string> FilterableBuckets()
        {
            return this.bucketRepository.Buckets
                .Where(b => b.Active)
                .Select(b => b.Code)
                .Union(new[] { string.Empty, UncategorisedFilter })
                .OrderBy(b => b);
        }

        public ObservableCollection<Transaction> FilterByBucket(string bucketCode)
        {
            this.transactions = new ObservableCollection<Transaction>(
                StatementModel.Transactions
                    .Where(t => MatchTransactionBucket(t, bucketCode)));
            return this.transactions;
        }

        public ObservableCollection<Transaction> FilterBySearchText(string searchText)
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

        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            StatementModel.Filter(criteria);
        }

        public async Task ImportAndMergeBankStatementAsync(string storageKey, Account.Account account)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            StatementModel additionalModel = await this.statementRepository.ImportBankStatementAsync(storageKey, account);
            StatementModel.Merge(additionalModel);
        }

        public void Initialise(StatementApplicationStateV1 stateData)
        {
            if (stateData == null)
            {
                throw new ArgumentNullException(nameof(stateData));
            }

            this.budgetHash = 0;
            this.sortedByBucket = stateData.SortByBucket ?? false;
        }

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            try
            {
                StatementModel = await this.statementRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.StatementModelStorageKey));
            }
            catch (StatementModelChecksumException ex)
            {
                throw new DataFormatException("Statement Model data is corrupt and has been tampered with. Unable to load.", ex);
            }

            ResetTransactionsCollection();

            EventHandler handler = NewDataSourceAvailable;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<TransactionGroupedByBucket> PopulateGroupByBucketCollection(bool groupByBucket)
        {
            this.sortedByBucket = groupByBucket;
            if (StatementModel == null)
            {
                // This can occur if the statement file is closed while viewing in GroupByBucket Mode.
                return new TransactionGroupedByBucket[] { };
            }

            if (this.sortedByBucket)
            {
                // SortByBucket == true so group and sort by bucket.
                IEnumerable<TransactionGroupedByBucket> query = StatementModel.Transactions
                    .GroupBy(t => t.BudgetBucket)
                    .OrderBy(g => g.Key)
                    .Select(group => new TransactionGroupedByBucket(group, group.Key));
                return new List<TransactionGroupedByBucket>(query);
            }
            // When viewing transactions by date, databinding pulls data directly from the StatementModel.
            // As for the GroupByBucket Collection this can be cleared.
            return new TransactionGroupedByBucket[] { };
        }

        public StatementApplicationStateV1 PreparePersistentStateData()
        {
            return new StatementApplicationStateV1
            {
                SortByBucket = this.sortedByBucket
            };
        }

        public void RemoveTransaction(Transaction transactionToRemove)
        {
            if (transactionToRemove == null)
            {
                throw new ArgumentNullException(nameof(transactionToRemove));
            }

            StatementModel.RemoveTransaction(transactionToRemove);
        }

        public async Task SaveAsync(IReadOnlyDictionary<ApplicationDataType, object> contextObjects)
        {
            if (StatementModel == null)
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

            await this.statementRepository.SaveAsync(StatementModel);
            EventHandler savedHandler = Saved;
            savedHandler?.Invoke(this, EventArgs.Empty);
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
        }

        public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
        {
            if (originalTransaction == null)
            {
                throw new ArgumentNullException(nameof(originalTransaction));
            }

            if (splinterBucket1 == null)
            {
                throw new ArgumentNullException(nameof(splinterBucket1));
            }

            if (splinterBucket2 == null)
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

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            handler?.Invoke(this, new ValidatingEventArgs());

            // In the case of the StatementModel all edits are validated and resolved during data edits. No need for an overall consistency check.
            return true;
        }

        public async Task<bool> ValidateWithCurrentBudgetsAsync(BudgetCollection budgets = null)
        {
            // This method must be called at least once with a budget collection.  Second and subsequent times do not require the budget.
            if (this.budgetCollection == null && budgets == null)
            {
                throw new ArgumentNullException(nameof(budgets));
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
            bool allTransactionHaveABucket = await Task.Run(
                () =>
                {
                    return StatementModel.AllTransactions
                        .Where(t => t.BudgetBucket != null)
                        .AsParallel()
                        .All(
                            t =>
                            {
                                bool bucketExists = allBuckets.Contains(t.BudgetBucket);
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

        private static bool MatchTransactionBucket(Transaction t, string bucketCode)
        {
            if (bucketCode.IsNothing())
            {
                return true;
            }

            if (bucketCode == UncategorisedFilter)
            {
                return t.BudgetBucket == null;
            }

            return t.BudgetBucket != null && t.BudgetBucket.Code == bucketCode;
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

        private void ResetTransactionsCollection()
        {
            this.transactions = StatementModel == null ? new ObservableCollection<Transaction>() : new ObservableCollection<Transaction>(StatementModel.Transactions);
        }
    }
}