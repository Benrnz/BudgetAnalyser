using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetBucketRepository
    {
        IEnumerable<BudgetBucket> Buckets { get; }
        BudgetBucket SurplusBucket { get; }

        BudgetBucket GetByCode(string code);

        BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom Collection")]
        void Initialise(BudgetCollection budgetCollectionModel);

        bool IsValidCode(string code);
    }
}