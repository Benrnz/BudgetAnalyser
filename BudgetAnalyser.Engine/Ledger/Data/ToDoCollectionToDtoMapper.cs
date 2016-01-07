using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    internal class Mapper_ListToDoTaskDto_ToDoCollection : IDtoMapper<List<ToDoTaskDto>, ToDoCollection>
    {
        public List<ToDoTaskDto> ToDto(ToDoCollection model)
        {
            var dto = new List<ToDoTaskDto>();
            var mapper = new Mapper_ToDoTaskDto_ToDoTask();
            dto.AddRange(model.Select(m => mapper.ToDto(m)));
            return dto;
        }

        public ToDoCollection ToModel(List<ToDoTaskDto> dto)
        {
            var model = new ToDoCollection();
            var mapper = new Mapper_ToDoTaskDto_ToDoTask();
            dto.ForEach(d => model.Add(mapper.ToModel(d)));
            return model;
        }
    }
}