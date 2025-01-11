using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data;

[AutoRegisterWithIoC]
internal class MapperToDoCollectionToDto2 : IDtoMapper<List<ToDoTaskDto>, ToDoCollection>
{
    private readonly IDtoMapper<ToDoTaskDto, ToDoTask> taskMapper = new MapperToDoTaskToDto2();

    public List<ToDoTaskDto> ToDto(ToDoCollection model)
    {
        var dto = model.Select(this.taskMapper.ToDto).ToList();
        return dto;
    }

    public ToDoCollection ToModel(List<ToDoTaskDto> dto)
    {
        var tasks = new ToDoCollection();
        dto.ForEach(t => tasks.Add(this.taskMapper.ToModel(t)));
        return tasks;
    }
}

internal class MapperToDoTaskToDto2 : IDtoMapper<ToDoTaskDto, ToDoTask>
{
    public ToDoTaskDto ToDto(ToDoTask model)
    {
        var dto = new ToDoTaskDto { Description = model.Description, CanDelete = model.CanDelete, SystemGenerated = model.SystemGenerated };
        return dto;
    }

    public ToDoTask ToModel(ToDoTaskDto dto)
    {
        var toDoTask = new ToDoTask { CanDelete = dto.CanDelete, Description = dto.Description, SystemGenerated = dto.SystemGenerated };
        return toDoTask;
    }
}
