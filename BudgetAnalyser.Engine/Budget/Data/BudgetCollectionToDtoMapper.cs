using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetCollection, BudgetCollectionDto>))]
    public class BudgetCollectionToDtoMapper : BasicMapper<BudgetCollection, BudgetCollectionDto>
    {
        private readonly BudgetModelToDtoMapper budgetModelMapper;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly BudgetBucketToDtoMapper bucketMapper;

        public BudgetCollectionToDtoMapper(
            [NotNull] BudgetModelToDtoMapper budgetModelMapper, 
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] BudgetBucketToDtoMapper bucketMapper)
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
        public override BudgetCollectionDto Map([NotNull] BudgetCollection budgetCollection)
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