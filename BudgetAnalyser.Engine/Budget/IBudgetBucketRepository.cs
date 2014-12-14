using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetBucketRepository
    {
        IEnumerable<BudgetBucket> Buckets { get; }
        BudgetBucket SurplusBucket { get; }

        /// <summary>
        /// Creates the new fixed budget project.
        /// </summary>
        /// <param name="bucketCode">The bucket code.</param>
        /// <param name="description">The description.</param>
        /// <param name="fixedBudgetAmount">The fixed budget amount.</param>
        /// <exception cref="ArgumentException">Will be thrown if the bucket code already exists.</exception>
        FixedBudgetProjectBucket CreateNewFixedBudgetProject([NotNull] string bucketCode, [NotNull] string description, decimal fixedBudgetAmount);
        BudgetBucket GetByCode(string code);
        BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory);
        void Initialise(IEnumerable<BudgetBucketDto> buckets);
        bool IsValidCode(string code);
    }
}