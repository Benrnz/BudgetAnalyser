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
    [AutoRegisterWithIoC]
    public class TransactionManagerService : ITransactionManagerService
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;
        private readonly IStatementRepository statementRepository;
        private BudgetCollection budgetCollection;
        private int budgetHash;
        private string currentTextFilter;
        private bool sortedByBucket;
        private StatementModel statementModel;

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
                if (this.statementModel == null || this.statementModel.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(this.currentTextFilter))
                {
                    IEnumerable<Transaction> query = this.statementModel.Transactions.Where(t => t.Amount < 0).ToList();
                    if (query.Any())
                    {
                        return query.Average(t => t.Amount);
                    }
                }

                if (this.currentTextFilter == UncategorisedFilter)
                {
                    return this.statementModel.Transactions
                        .Where(t => t.BudgetBucket == null && t.Amount < 0)
                        .Average(t => t.Amount);
                }

                return this.statementModel.Transactions
                    .Where(t => t.Amount < 0 && t.BudgetBucket != null && t.BudgetBucket.Code == this.currentTextFilter)
                    .Average(t => t.Amount);
            }
        }

        public decimal TotalCount
        {
            get
            {
                if (this.statementModel == null || this.statementModel.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(this.currentTextFilter))
                {
                    return this.statementModel.Transactions.Count();
                }

                if (this.currentTextFilter == UncategorisedFilter)
                {
                    return this.statementModel.Transactions.Count(t => t.BudgetBucket == null);
                }

                return this.statementModel.Transactions.Count(t => t.BudgetBucket != null && t.BudgetBucket.Code == this.currentTextFilter);
            }
        }

        public decimal TotalCredits
        {
            get
            {
                if (this.statementModel == null || this.statementModel.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(this.currentTextFilter))
                {
                    return this.statementModel.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                }

                if (this.currentTextFilter == UncategorisedFilter)
                {
                    return this.statementModel.Transactions.Where(t => t.BudgetBucket == null && t.Amount > 0).Sum(t => t.Amount);
                }

                return this.statementModel.Transactions
                    .Where(t => t.Amount > 0 && t.BudgetBucket != null && t.BudgetBucket.Code == this.currentTextFilter)
                    .Sum(t => t.Amount);
            }
        }

        public decimal TotalDebits
        {
            get
            {
                if (this.statementModel == null || this.statementModel.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(this.currentTextFilter))
                {
                    return this.statementModel.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                }

                if (this.currentTextFilter == UncategorisedFilter)
                {
                    return this.statementModel.Transactions
                        .Where(t => t.BudgetBucket == null && t.Amount < 0)
                        .Sum(t => t.Amount);
                }

                return this.statementModel.Transactions
                    .Where(t => t.Amount < 0 && t.BudgetBucket != null && t.BudgetBucket.Code == this.currentTextFilter)
                    .Sum(t => t.Amount);
            }
        }

        public string DetectDuplicateTransactions()
        {
            if (this.statementModel == null)
            {
                return null;
            }

            var duplicates = this.statementModel.ValidateAgainstDuplicates().ToList();
            return duplicates.Any()
                ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!", duplicates.Sum(group => group.Count()))
                : null;
        }

        public IEnumerable<string> FilterableBuckets()
        {
            return this.bucketRepository.Buckets
                .Select(b => b.Code)
                .Union(new[] { string.Empty, UncategorisedFilter })
                .OrderBy(b => b);
        }

        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            this.currentTextFilter = null;
            this.statementModel.Filter(criteria);
        }

        public void FilterTransactions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentNullException("searchText");
            }

            this.currentTextFilter = searchText;
            this.statementModel.FilterByText(searchText);
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
            var additionalModel = this.statementRepository.ImportAndMergeBankStatement(storageKey, account);
            this.statementModel.Merge(additionalModel);
        }

        public StatementApplicationState LoadPersistedStateData(object stateData)
        {
            this.budgetHash = 0;
            var state = (StatementApplicationState)stateData;
            this.sortedByBucket = state.SortByBucket ?? false;
            return state;
        }

        public async Task<StatementModel> LoadStatementModelAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            this.statementModel = await this.statementRepository.LoadStatementModelAsync(storageKey);
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
                var query = this.statementModel.Transactions
                    .GroupBy(t => t.BudgetBucket)
                    .OrderBy(g => g.Key)
                    .Select(group => new TransactionGroupedByBucket(group, group.Key));
                return new List<TransactionGroupedByBucket>(query);
            }
            // When viewing transactions by date, databinding pulls data directly from the StatementModel.
            // As for the GroupByBucket Collection this can be cleared.
            return new TransactionGroupedByBucket[] { };
        }

        public object PreparePersistentStateData()
        {
            return new StatementApplicationState
            {
                StorageKey = this.statementModel == null ? null : this.statementModel.StorageKey,
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
            var allTransactionHaveABucket = await Task.Run(
                () =>
                {
                    return this.statementModel.AllTransactions
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
    }
}