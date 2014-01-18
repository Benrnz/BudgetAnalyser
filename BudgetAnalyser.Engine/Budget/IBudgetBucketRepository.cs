using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetBucketRepository
    {
        IEnumerable<BudgetBucket> Buckets { get; }
        BudgetBucket SurplusBucket { get; }

        BudgetBucket GetByCode(string code);

        BudgetBucket GetOrAdd(string code, Func<BudgetBucket> factory);

        void Initialise(BudgetCollection budgetCollectionModel);

        bool IsValidCode(string code);
    }
}