using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.Statement;
using System;

namespace BudgetAnalyser.Engine.Statement.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 1:54:58 AM UTC")]
    internal partial class Mapper_TransactionSetDto_StatementModel : IDtoMapper<TransactionSetDto, StatementModel>
    {

        public virtual StatementModel ToModel(TransactionSetDto dto)
        {
            ToModelPreprocessing(dto);
            StatementModel model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(StatementModel).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (StatementModel)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var lastImport4 = dto.LastImport;
            modelType.GetProperty("LastImport").SetValue(model, lastImport4);
            var storageKey5 = dto.StorageKey;
            model.StorageKey = storageKey5;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual TransactionSetDto ToDto(StatementModel model)
        {
            ToDtoPreprocessing(model);
            TransactionSetDto dto;
            dto = new TransactionSetDto();
            var lastImport8 = model.LastImport;
            dto.LastImport = lastImport8;
            var storageKey9 = model.StorageKey;
            dto.StorageKey = storageKey9;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(TransactionSetDto dto);
        partial void ToDtoPreprocessing(StatementModel model);
        partial void ModelFactory(TransactionSetDto dto, ref StatementModel model);
        partial void ToModelPostprocessing(TransactionSetDto dto, ref StatementModel model);
        partial void ToDtoPostprocessing(ref TransactionSetDto dto, StatementModel model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 1:41:11 AM UTC")]
    internal partial class Mapper_TransactionDto_Transaction : IDtoMapper<TransactionDto, Transaction>
    {

        public virtual Transaction ToModel(TransactionDto dto)
        {
            ToModelPreprocessing(dto);
            Transaction model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new Transaction();
            }
            var modelType = model.GetType();
            var amount2 = dto.Amount;
            model.Amount = amount2;
            var date4 = dto.Date;
            model.Date = date4;
            var description5 = dto.Description;
            model.Description = description5;
            var id6 = dto.Id;
            modelType.GetProperty("Id").SetValue(model, id6);
            var reference18 = dto.Reference1;
            model.Reference1 = reference18;
            var reference29 = dto.Reference2;
            model.Reference2 = reference29;
            var reference310 = dto.Reference3;
            model.Reference3 = reference310;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual TransactionDto ToDto(Transaction model)
        {
            ToDtoPreprocessing(model);
            TransactionDto dto;
            dto = new TransactionDto();
            var amount14 = model.Amount;
            dto.Amount = amount14;
            var date16 = model.Date;
            dto.Date = date16;
            var description17 = model.Description;
            dto.Description = description17;
            var id18 = model.Id;
            dto.Id = id18;
            var reference119 = model.Reference1;
            dto.Reference1 = reference119;
            var reference220 = model.Reference2;
            dto.Reference2 = reference220;
            var reference321 = model.Reference3;
            dto.Reference3 = reference321;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(TransactionDto dto);
        partial void ToDtoPreprocessing(Transaction model);
        partial void ModelFactory(TransactionDto dto, ref Transaction model);
        partial void ToModelPostprocessing(TransactionDto dto, ref Transaction model);
        partial void ToDtoPostprocessing(ref TransactionDto dto, Transaction model);
    } // End Class
} // End Namespace
