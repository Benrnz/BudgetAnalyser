namespace BudgetAnalyser.Engine.Persistence;

public interface IDtoMapper<TDto, TModel> where TDto : class where TModel : class
{
    TDto ToDto(TModel model);

    TModel ToModel(TDto dto);
}
