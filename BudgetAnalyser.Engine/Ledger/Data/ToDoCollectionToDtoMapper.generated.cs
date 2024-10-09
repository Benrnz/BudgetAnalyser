using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Ledger;
using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 7/01/2016 3:35:05 AM UTC")]
    internal partial class Mapper_ToDoTaskDto_ToDoTask : IDtoMapper<ToDoTaskDto, ToDoTask>
    {

        public virtual ToDoTask ToModel(ToDoTaskDto dto)
        {
            ToDoTask model = null;
            ModelFactory(dto, ref model);
            if (model is null)
            {
                var constructors = typeof(ToDoTask).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (ToDoTask)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            ToModelPreprocessing(dto, model);
            var canDelete1 = dto.CanDelete;
            modelType.GetProperty("CanDelete").SetValue(model, canDelete1);
            var description2 = dto.Description;
            modelType.GetProperty("Description").SetValue(model, description2);
            var systemGenerated3 = dto.SystemGenerated;
            modelType.GetProperty("SystemGenerated").SetValue(model, systemGenerated3);
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method
        public virtual ToDoTaskDto ToDto(ToDoTask model)
        {
            ToDoTaskDto dto = null;
            DtoFactory(ref dto, model);
            if (dto is null)
            {
                dto = new ToDoTaskDto();
            }
            ToDtoPreprocessing(dto, model);
            var canDelete4 = model.CanDelete;
            dto.CanDelete = canDelete4;
            var description5 = model.Description;
            dto.Description = description5;
            var systemGenerated6 = model.SystemGenerated;
            dto.SystemGenerated = systemGenerated6;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(ToDoTaskDto dto, ToDoTask model);
        partial void ToDtoPreprocessing(ToDoTaskDto dto, ToDoTask model);
        partial void ModelFactory(ToDoTaskDto dto, ref ToDoTask model);
        partial void DtoFactory(ref ToDoTaskDto dto, ToDoTask model);
        partial void ToModelPostprocessing(ToDoTaskDto dto, ref ToDoTask model);
        partial void ToDtoPostprocessing(ref ToDoTaskDto dto, ToDoTask model);
    } // End Class

} // End Namespace
