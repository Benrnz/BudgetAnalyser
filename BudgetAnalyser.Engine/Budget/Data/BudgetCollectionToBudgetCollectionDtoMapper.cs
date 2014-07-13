using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    public class BudgetCollectionToBudgetCollectionDtoMapper
    {
        private readonly BudgetModelToBudgetModelDtoMapper budgetModelMapper;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly BudgetBucketToBudgetBucketDtoMapper bucketMapper;

        public BudgetCollectionToBudgetCollectionDtoMapper(
            [NotNull] BudgetModelToBudgetModelDtoMapper budgetModelMapper, 
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] BudgetBucketToBudgetBucketDtoMapper bucketMapper)
        {
            if (budgetModelMapper == null)
            {
                throw new ArgumentNullException("budgetModelMapper");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }
            
            if (bucketMapper == null)
            {
                throw new ArgumentNullException("bucketMapper");
            }

            this.budgetModelMapper = budgetModelMapper;
            this.bucketRepo = bucketRepo;
            this.bucketMapper = bucketMapper;
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        public BudgetCollectionDto Map([NotNull] BudgetCollection budgetCollection)
        {
            if (budgetCollection == null)
            {
                throw new ArgumentNullException("budgetCollection");
            }

            var collection = new BudgetCollectionDto
            {
                FileName = budgetCollection.FileName,
                Budgets = budgetCollection.Select(b => this.budgetModelMapper.Map(b)).ToList(),
                Buckets = this.bucketRepo.Buckets.Select(b => this.bucketMapper.Map(b)).ToList(),
            };

            return collection;
        }
    }
}