namespace BudgetAnalyser.Engine.Persistence;

[AutoRegisterWithIoC]
public class MapperGlobalFilterCriteriaToDto2 : IDtoMapper<GlobalFilterDto, GlobalFilterCriteria>
{
    public GlobalFilterDto ToDto(GlobalFilterCriteria model)
    {
        return new GlobalFilterDto
        {
            BeginDate = model.BeginDate,
            EndDate = model.EndDate
        };
    }

    public GlobalFilterCriteria ToModel(GlobalFilterDto dto)
    {
        return new GlobalFilterCriteria { BeginDate = dto.BeginDate, EndDate = dto.EndDate };
    }
}
