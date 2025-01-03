﻿using System;
using System.Linq;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class MapperBudgetCollectionDtoBudgetCollection
    {
        private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper;

        public MapperBudgetCollectionDtoBudgetCollection(
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper,
            [NotNull] IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper)
        {
            if (bucketRepo is null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            if (bucketMapper is null)
            {
                throw new ArgumentNullException(nameof(bucketMapper));
            }

            if (budgetMapper is null)
            {
                throw new ArgumentNullException(nameof(budgetMapper));
            }

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
