using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Ledger;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Persistence;
using System;

namespace BudgetAnalyser.Engine.Persistence
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 7/01/2016 3:19:51 AM UTC")]
    internal partial class Mapper_BudgetAnalyserStorageRoot_ApplicationDatabase : IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase>
    {

        public virtual ApplicationDatabase ToModel(BudgetAnalyserStorageRoot dto)
        {
            ApplicationDatabase model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new ApplicationDatabase();
            }
            var modelType = model.GetType();
            ToModelPreprocessing(dto, model);
            model.IsEncrypted = dto.IsEncrypted;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method
        public virtual BudgetAnalyserStorageRoot ToDto(ApplicationDatabase model)
        {
            BudgetAnalyserStorageRoot dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new BudgetAnalyserStorageRoot();
            }
            ToDtoPreprocessing(dto, model);
            dto.IsEncrypted = model.IsEncrypted;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(BudgetAnalyserStorageRoot dto, ApplicationDatabase model);
        partial void ToDtoPreprocessing(BudgetAnalyserStorageRoot dto, ApplicationDatabase model);
        partial void ModelFactory(BudgetAnalyserStorageRoot dto, ref ApplicationDatabase model);
        partial void DtoFactory(ref BudgetAnalyserStorageRoot dto, ApplicationDatabase model);
        partial void ToModelPostprocessing(BudgetAnalyserStorageRoot dto, ref ApplicationDatabase model);
        partial void ToDtoPostprocessing(ref BudgetAnalyserStorageRoot dto, ApplicationDatabase model);
    } // End Class

} // End Namespace
