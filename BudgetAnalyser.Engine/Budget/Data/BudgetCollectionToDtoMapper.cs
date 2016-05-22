using System;
using System.Linq;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class Mapper_BudgetCollectionDto_BudgetCollection
    {
        private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper;

        public Mapper_BudgetCollectionDto_BudgetCollection(
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper,
            [NotNull] IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (bucketMapper == null) throw new ArgumentNullException(nameof(bucketMapper));
            if (budgetMapper == null) throw new ArgumentNullException(nameof(budgetMapper));
            this.bucketRepo = bucketRepo;
            this.bucketMapper = bucketMapper;
            this.budgetMapper = budgetMapper;
        }

        partial void ToDtoPostprocessing(ref BudgetCollectionDto dto, BudgetCollection model)
        {
            dto.Buckets = this.bucketRepo.Buckets.Select(b => this.bucketMapper.ToDto(b)).ToList();
            dto.Budgets = model.ToList().Select(x => this.budgetMapper.ToDto(x)).ToList();
        }

        partial void ToModelPostprocessing(BudgetCollectionDto dto, ref BudgetCollection model)
        {
            var budgetCollection = model;
            dto.Budgets.ForEach(x => budgetCollection.Add(this.budgetMapper.ToModel(x)));
            dto.Buckets.ForEach(x => this.bucketRepo.GetOrCreateNew(x.Code, () => this.bucketMapper.ToModel(x)));
        }
    }
}