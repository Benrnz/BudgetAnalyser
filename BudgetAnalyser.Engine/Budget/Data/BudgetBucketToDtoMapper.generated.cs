using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Budget;
using System;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 12:39:29 AM UTC")]
    internal partial class Mapper_BudgetBucketDto_BudgetBucket : IDtoMapper<BudgetBucketDto, BudgetBucket>
    {

        public virtual BudgetBucket ToModel(BudgetBucketDto dto)
        {
            ToModelPreprocessing(dto);
            BudgetBucket model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(BudgetBucket).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (BudgetBucket)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var active1 = dto.Active;
            model.Active = active1;
            var code2 = dto.Code;
            model.Code = code2;
            var description3 = dto.Description;
            model.Description = description3;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual BudgetBucketDto ToDto(BudgetBucket model)
        {
            ToDtoPreprocessing(model);
            BudgetBucketDto dto;
            dto = new BudgetBucketDto();
            var active4 = model.Active;
            dto.Active = active4;
            var code5 = model.Code;
            dto.Code = code5;
            var description6 = model.Description;
            dto.Description = description6;
            // var type7 = // TODO Cannot find a way to retrieve this property: model.Type. 
            // dto.Type = type7; // TODO Cannot find a way to set this property: dto.Type. 
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(BudgetBucketDto dto);
        partial void ToDtoPreprocessing(BudgetBucket model);
        partial void ModelFactory(BudgetBucketDto dto, ref BudgetBucket model);
        partial void ToModelPostprocessing(BudgetBucketDto dto, ref BudgetBucket model);
        partial void ToDtoPostprocessing(ref BudgetBucketDto dto, BudgetBucket model);
    } // End Class

} // End Namespace
