using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetBucketRepository : IBudgetBucketRepository
    {
        private const string JournalCode = "JOURNAL";
        private const string SurplusCode = "SURPLUS";

        private readonly object syncRoot = new object();
        private Dictionary<string, BudgetBucket> lookupTable = new Dictionary<string, BudgetBucket>();
        private bool subscribedToBudgetValidation;

        public IEnumerable<BudgetBucket> Buckets
        {
            get { return this.lookupTable.Values.OrderBy(b => b.Code).ToList(); }
        }

        public BudgetBucket SurplusBucket { get; private set; }

        public BudgetBucket GetByCode(string code)
        {
            if (IsValidCode(code))
            {
                return this.lookupTable[code];
            }

            return null;
        }

        public BudgetBucket GetOrAdd(string code, Func<BudgetBucket> factory)
        {
            if (IsValidCode(code))
            {
                return this.lookupTable[code];
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(code))
                {
                    return this.lookupTable[code];
                }

                BudgetBucket newBucket = factory();
                this.lookupTable.Add(code, newBucket);
                return newBucket;
            }
        }

        public void Initialise(BudgetCollection budgetCollectionModel)
        {
            if (!this.subscribedToBudgetValidation)
            {
                budgetCollectionModel.Validating += (s, e) => Initialise(s as BudgetCollection);
                this.subscribedToBudgetValidation = true;
            }

            Dictionary<string, BudgetBucket> buckets = budgetCollectionModel.SelectMany(model => model.Expenses).Distinct().ToDictionary(e => e.Bucket.Code, e => e.Bucket);

            SurplusBucket = new SurplusBucket(SurplusCode, "A special bucket to allocate against any discretionary spending.");
            buckets.Add(SurplusCode, SurplusBucket);

            foreach (Income income in budgetCollectionModel.SelectMany(model => model.Incomes)
                .Distinct())
            {
                buckets.Add(income.Bucket.Code, income.Bucket);
            }

            buckets.Add(JournalCode, new JournalBucket(JournalCode, "A special bucket to allocate against internal transfers."));

            this.lookupTable = buckets;
        }

        public bool IsValidCode(string code)
        {
            return this.lookupTable.ContainsKey(code);
        }
    }
}