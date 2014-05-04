using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A thread-safe in memory implementation of <see cref="IBudgetBucketRepository" />.
    ///     This repository does not need to be persisted, because it uses the <see cref="BudgetCollection" /> as the source of
    ///     truth.
    ///     Thread safety is built in to allow multiple threads to load data from statements asynchronously.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryBudgetBucketRepository : IBudgetBucketRepository
    {
        private readonly object syncRoot = new object();
        private Dictionary<string, BudgetBucket> lookupTable = new Dictionary<string, BudgetBucket>();
        private bool subscribedToBudgetValidation;

        public IEnumerable<BudgetBucket> Buckets
        {
            get { return this.lookupTable.Values.OrderBy(b => b.Code).ToList(); }
        }

        public BudgetBucket SurplusBucket { get; private set; }

        public BudgetBucket GetByCode([NotNull] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            string upperCode = code.ToUpperInvariant();
            if (IsValidCode(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            return null;
        }

        public BudgetBucket GetOrAdd([NotNull] string code, [NotNull] Func<BudgetBucket> factory)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            string upperCode = code.ToUpperInvariant();
            if (IsValidCode(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(upperCode))
                {
                    return this.lookupTable[upperCode];
                }

                BudgetBucket newBucket = factory();
                this.lookupTable.Add(upperCode, newBucket);
                return newBucket;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        public void Initialise([NotNull] BudgetCollection budgetCollectionModel)
        {
            if (budgetCollectionModel == null)
            {
                throw new ArgumentNullException("budgetCollectionModel");
            }

            if (!this.subscribedToBudgetValidation)
            {
                budgetCollectionModel.Validating += (s, e) => Initialise(s as BudgetCollection);
                this.subscribedToBudgetValidation = true;
            }

            Dictionary<string, BudgetBucket> buckets = budgetCollectionModel
                .SelectMany(model => model.Expenses)
                .Distinct()
                .ToDictionary(e => e.Bucket.Code, e => e.Bucket);

            SurplusBucket = new SurplusBucket();
            buckets.Add(Budget.SurplusBucket.SurplusCode, SurplusBucket);

            buckets.Add(JournalBucket.JournalCode, new JournalBucket(JournalBucket.JournalCode, "A special bucket to allocate against internal transfers."));

            foreach (Income income in budgetCollectionModel
                .SelectMany(model => model.Incomes)
                .Distinct())
            {
                buckets.Add(income.Bucket.Code, income.Bucket);
            }

            this.lookupTable = buckets;
        }

        public bool IsValidCode([NotNull] string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            return this.lookupTable.ContainsKey(code.ToUpperInvariant());
        }
    }
}