using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Reflection;
    using Rees.TangyFruitMapper;
    using BudgetAnalyser.Engine.Budget;
    using System.Collections.Generic;
    using System;
    using BudgetAnalyser.Engine.Budget.Data;


    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    internal class Mapper_BudgetCollectionDto_BudgetCollection : IDtoMapper<BudgetCollectionDto, BudgetCollection>
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_BudgetCollectionDto_BudgetCollection([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        public BudgetCollection ToModel(BudgetCollectionDto dto)
        {
            var model = new BudgetCollection();
            var source26 = dto.StorageKey;
            model.StorageKey = source26;
            //var budgetModelMapper = new Mapper_BudgetModelDto_BudgetModel();
            //dto.Budgets.ForEach(x => model.Add(budgetModelMapper.ToModel(x)));
            return model;
        } // End ToModel Method

        public BudgetCollectionDto ToDto(BudgetCollection model)
        {
            //var budgetModelMapper = new Mapper_BudgetModelDto_BudgetModel();
            var bucketMapper = new Mapper_BudgetBucketDto_BudgetBucket();

            var dto = new BudgetCollectionDto();
            var source27 = this.bucketRepo.Buckets;
            dto.Buckets = source27.Select(b => bucketMapper.ToDto(b)).ToList(); 
            //dto.Budgets = model.ToList().Select(x => budgetModelMapper.ToDto(x)).ToList();
            var source29 = model.StorageKey;
            dto.StorageKey = source29;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace
