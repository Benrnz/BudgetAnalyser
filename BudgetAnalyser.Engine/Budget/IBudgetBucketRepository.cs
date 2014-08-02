using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetBucketRepository
    {
        IEnumerable<BudgetBucket> Buckets { get; }
        BudgetBucket SurplusBucket { get; }

        BudgetBucket GetByCode(string code);

        BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory);

        void Initialise(IEnumerable<BudgetBucketDto> buckets);

        bool IsValidCode(string code);
    }
}