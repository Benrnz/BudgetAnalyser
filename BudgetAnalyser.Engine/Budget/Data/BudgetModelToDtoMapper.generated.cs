using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Budget;
using System.Collections.Generic;
using System;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 12:48:10 AM UTC")]
    internal partial class Mapper_BudgetModelDto_BudgetModel : IDtoMapper<BudgetModelDto, BudgetModel>
    {

        public virtual BudgetModel ToModel(BudgetModelDto dto)
        {
            ToModelPreprocessing(dto);
            BudgetModel model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new BudgetModel();
            }
            var modelType = model.GetType();
            var effectiveFrom1 = dto.EffectiveFrom;
            model.EffectiveFrom = effectiveFrom1;
            var lastModifiedComment5 = dto.LastModifiedComment;
            model.LastModifiedComment = lastModifiedComment5;
            var name6 = dto.Name;
            model.Name = name6;
            model.BudgetCycle = dto.BudgetCycle;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual BudgetModelDto ToDto(BudgetModel model)
        {
            ToDtoPreprocessing(model);
            BudgetModelDto dto;
            dto = new BudgetModelDto();
            var effectiveFrom7 = model.EffectiveFrom;
            dto.EffectiveFrom = effectiveFrom7;
            var lastModified10 = model.LastModified;
            dto.LastModified = lastModified10;
            var lastModifiedComment11 = model.LastModifiedComment;
            dto.LastModifiedComment = lastModifiedComment11;
            var name12 = model.Name;
            dto.Name = name12;
            dto.BudgetCycle = model.BudgetCycle;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(BudgetModelDto dto);
        partial void ToDtoPreprocessing(BudgetModel model);
        partial void ModelFactory(BudgetModelDto dto, ref BudgetModel model);
        partial void ToModelPostprocessing(BudgetModelDto dto, ref BudgetModel model);
        partial void ToDtoPostprocessing(ref BudgetModelDto dto, BudgetModel model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 12:48:10 AM UTC")]
    internal partial class Mapper_ExpenseDto_Expense : IDtoMapper<ExpenseDto, Expense>
    {

        public virtual Expense ToModel(ExpenseDto dto)
        {
            ToModelPreprocessing(dto);
            Expense model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new Expense();
            }
            var modelType = model.GetType();
            var amount13 = dto.Amount;
            model.Amount = amount13;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual ExpenseDto ToDto(Expense model)
        {
            ToDtoPreprocessing(model);
            ExpenseDto dto;
            dto = new ExpenseDto();
            var amount15 = model.Amount;
            dto.Amount = amount15;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(ExpenseDto dto);
        partial void ToDtoPreprocessing(Expense model);
        partial void ModelFactory(ExpenseDto dto, ref Expense model);
        partial void ToModelPostprocessing(ExpenseDto dto, ref Expense model);
        partial void ToDtoPostprocessing(ref ExpenseDto dto, Expense model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 12:48:10 AM UTC")]
    internal partial class Mapper_IncomeDto_Income : IDtoMapper<IncomeDto, Income>
    {

        public virtual Income ToModel(IncomeDto dto)
        {
            ToModelPreprocessing(dto);
            Income model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new Income();
            }
            var modelType = model.GetType();
            var amount17 = dto.Amount;
            model.Amount = amount17;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual IncomeDto ToDto(Income model)
        {
            ToDtoPreprocessing(model);
            IncomeDto dto;
            dto = new IncomeDto();
            var amount19 = model.Amount;
            dto.Amount = amount19;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(IncomeDto dto);
        partial void ToDtoPreprocessing(Income model);
        partial void ModelFactory(IncomeDto dto, ref Income model);
        partial void ToModelPostprocessing(IncomeDto dto, ref Income model);
        partial void ToDtoPostprocessing(ref IncomeDto dto, Income model);
    } // End Class

} // End Namespace
