namespace Rees.TangyFruitMapper
{
    /// <summary>
    ///     A basic mapping interface showing the minimum functionality required to map two objects.
    /// </summary>
    /// <typeparam name="TDto">The type of the dto.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public interface IDtoMapper<TDto, TModel>
    {
        /// <summary>
        ///     Maps the <paramref name="model" /> to a dto.
        /// </summary>
        TDto ToDto(TModel model);

        /// <summary>
        ///     Maps the <paramref name="dto" /> to a model.
        /// </summary>
        TModel ToModel(TDto dto);
    }
}