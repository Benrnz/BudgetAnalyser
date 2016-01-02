using System;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class DtoMapperStub<TSource, TDestination> : IDtoMapper<TSource, TDestination>
        where TDestination : class
        where TSource : class
    {
        /// <summary>
        ///     Maps the <paramref name="model" /> to a dto.
        /// </summary>
        public TSource ToDto(TDestination model)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Maps the <paramref name="dto" /> to a model.
        /// </summary>
        public TDestination ToModel(TSource dto)
        {
            throw new NotSupportedException();
        }
    }
}