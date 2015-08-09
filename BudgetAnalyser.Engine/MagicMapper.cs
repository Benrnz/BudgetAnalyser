using System;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An AutoMapper implementation of mapping one type to another. This relies on all AutoMapper maps being pre-created
    ///     beforehand (preferably application start up).
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The desired destination type to transform the source into.</typeparam>
    public abstract class MagicMapper<TSource, TDestination> : BasicMapper<TSource, TDestination>
        where TSource : class
        where TDestination : class
    {
        /// <summary>
        ///     Map from source to destination types. The source remains unchanged.
        ///     It is not necessary to override this method in a derived class unless additional is required after the data is
        ///     copied from the source into the destination.
        ///     This will fail if AutoMapper has not be given configuration (CreateMap) previously.
        /// </summary>
        /// <param name="source">The source instance to transform</param>
        /// <returns>
        ///     A new instance of <typeparamref name="TDestination" /> mapped from <paramref name="source" />. All properties will
        ///     be populated in accordance with
        ///     the AutoMapper configuration.
        /// </returns>
        public override TDestination Map([NotNull] TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return Mapper.Map<TDestination>(source);
        }
    }
}