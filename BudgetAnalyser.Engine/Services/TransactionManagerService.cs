using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class TransactionManagerService : ITransactionManagerService
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;
        private readonly IStatementRepository statementRepository;
        private BudgetCollection budgetCollection;
        private int budgetHash;
        private bool sortedByBucket;
        private StatementModel statementModel;
        private ObservableCollection<Transaction> transactions;

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

        public decimal AverageDebit
        {
            get
            {
                if (this.transactions == null)
                {
                    return 0;
                }

                return this.transactions.Where(t => t.Amount < 0).Average(t => t.Amount);
            }
        }

        public decimal TotalCount
        {
            get
            {
                if (this.transactions == null)
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
                if (this.transactions == null)
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
                if (this.transactions == null)
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

        public string DetectDuplicateTransactions()
        {
            if (this.statementModel == null)
            {
                return null;
            }

            List<IGrouping<int, Transaction>> duplicates = this.statementModel.ValidateAgainstDuplicates().ToList();
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
                this.statementModel.Transactions
                    .Where(t => MatchTransactionBucket(t, bucketCode)));
            return this.transactions;
        }

        public ObservableCollection<Transaction> FilterBySearchText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return ClearBucketAndTextFilters();
            }

            if (searchText.Length < 3)
            {
                return ClearBucketAndTextFilters();
            }

            this.transactions = new ObservableCollection<Transaction>(
                this.statementModel.Transactions.Where(t => MatchTransactionText(t, searchText))
                    .AsParallel()
                    .ToList());
            return this.transactions;
        }

        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            this.statementModel.Filter(criteria);
        }

        public void ImportAndMergeBankStatement(string storageKey, AccountType account)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            // TODO should be async
            StatementModel additionalModel = this.statementRepository.ImportAndMergeBankStatement(storageKey, account);
            this.statementModel.Merge(additionalModel);
        }

        public void Initialise(StatementApplicationStateV1 stateData)
        {
            if (stateData == null)
            {
                throw new ArgumentNullException("stateData");
            }
            this.budgetHash = 0;
            this.sortedByBucket = stateData.SortByBucket ?? false;
        }

        public async Task<StatementModel> LoadStatementModelAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            this.statementModel = await this.statementRepository.LoadStatementModelAsync(storageKey);
            ResetTransactionsCollection();
            return this.statementModel;
        }

        public IEnumerable<TransactionGroupedByBucket> PopulateGroupByBucketCollection(bool groupByBucket)
        {
            this.sortedByBucket = groupByBucket;
            if (this.statementModel == null)
            {
                // This can occur if the statement file is closed while viewing in GroupByBucket Mode.
                return new TransactionGroupedByBucket[] { };
            }

            if (this.sortedByBucket)
            {
                // SortByBucket == true so group and sort by bucket.
                IEnumerable<TransactionGroupedByBucket> query = this.statementModel.Transactions
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
                throw new ArgumentNullException("transactionToRemove");
            }

            this.statementModel.RemoveTransaction(transactionToRemove);
        }

        public async Task SaveAsync(bool close)
        {
            if (this.statementModel == null)
            {
                return;
            }

            await this.statementRepository.SaveAsync(this.statementModel);
            if (close)
            {
                this.statementModel = null;
            }
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

            this.statementModel.SplitTransaction(
                originalTransaction,
                splinterAmount1,
                splinterAmount2,
                splinterBucket1,
                splinterBucket2);
        }

        public async Task<bool> ValidateWithCurrentBudgetsAsync(BudgetCollection budgets = null)
        {
            // This method must be called at least once with a budget collection.  Second and subsequent times do not require the budget.
            if (this.budgetCollection == null && budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            this.budgetCollection = budgets ?? this.budgetCollection;

            if (this.statementModel == null)
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
                    return this.statementModel.AllTransactions
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

        private void ResetTransactionsCollection()
        {
            this.transactions = new ObservableCollection<Transaction>(this.statementModel.Transactions);
        }

        private static bool MatchTransactionBucket(Transaction t, string bucketCode)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
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
            if (!String.IsNullOrWhiteSpace(t.Description))
            {
                if (t.Description.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!String.IsNullOrWhiteSpace(t.Reference1))
            {
                if (t.Reference1.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!String.IsNullOrWhiteSpace(t.Reference2))
            {
                if (t.Reference2.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (!String.IsNullOrWhiteSpace(t.Reference3))
            {
                if (t.Reference3.IndexOf(textFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}