namespace BudgetAnalyser.Engine.Budget.Data
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Reflection;
    using Rees.TangyFruitMapper;
    using BudgetAnalyser.Engine.Budget;
    using System;
    using BudgetAnalyser.Engine.Budget.Data;

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    internal class Mapper_BudgetBucketDto_BudgetBucket : IDtoMapper<BudgetBucketDto, BudgetBucket>
    {

        public BudgetBucket ToModel(BudgetBucketDto dto)
        {
            var model = new BudgetBucketFactory().Build(dto.Type);
            var modelType = model.GetType();
            var active1 = dto.Active;
            model.Active = active1;
            var code2 = dto.Code;
            model.Code = code2;
            var description3 = dto.Description;
            model.Description = description3;
            return model;
        } // End ToModel Method

        public BudgetBucketDto ToDto(BudgetBucket model)
        {
            var dto = new BudgetBucketDto();
            var active5 = model.Active;
            dto.Active = active5;
            var code6 = model.Code;
            dto.Code = code6;
            var description7 = model.Description;
            dto.Description = description7;
            dto.Type = new BudgetBucketFactory().SerialiseType(model);
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace
