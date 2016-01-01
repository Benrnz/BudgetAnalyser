
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System.Collections.Generic;
using System;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Budget;

namespace GeneratedCode
{
    
    [GeneratedCode("1.0", "Tangy Fruit Mapper")] 
    public partial class Mapper_BudgetCollectionDto_BudgetCollection : IDtoMapper<BudgetCollectionDto, BudgetCollection>
    {
        
        public BudgetCollection ToModel(BudgetCollectionDto dto)
        {
            var model = new BudgetCollection();
            var modelType = model.GetType();
            // var source1 = // TODO Cannot find a way to retrieve this property: dto.Count. 
            // model.Count = source1; // TODO Cannot find a way to set this property: model.Count. 
            // var source2 = // TODO Cannot find a way to retrieve this property: dto.CurrentActiveBudget. 
            // model.CurrentActiveBudget = source2; // TODO Cannot find a way to set this property: model.CurrentActiveBudget. 
            var source3 = dto.StorageKey;
            model.StorageKey = source3;
            return model;
        } // End ToModel Method

        public BudgetCollectionDto ToDto(BudgetCollection model)
        {
            var dto = new BudgetCollectionDto();
            // var source4 = // TODO Cannot find a way to retrieve this property: model.Buckets. 
            // dto.Buckets = source4; // TODO Cannot find a way to set this property: dto.Buckets. 
            // var source5 = // TODO Cannot find a way to retrieve this property: model.Budgets. 
            // dto.Budgets = source5; // TODO Cannot find a way to set this property: dto.Budgets. 
            var source6 = model.StorageKey;
            dto.StorageKey = source6;
            return dto;
        } // End ToDto Method

        partial void ToModelPreprocessing(BudgetBucketDto dto);

    } // End Class

} // End Namespace

