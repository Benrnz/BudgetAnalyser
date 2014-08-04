namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An abstract Mapping class that defines the high level interface for any mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The desired destination type to transform the source into.</typeparam>
    public abstract class BasicMapper<TSource, TDestination> 
        where TSource : class 
        where TDestination : class
    {
        /// <summary>
        /// Map from source to destination types. The source remains unchanged.
        /// </summary>
        /// <param name="source">The source instance to transform</param>
        /// <returns>A new instance of <typeparamref name="TDestination"/> mapped from <paramref name="source"/>.</returns>
        public abstract TDestination Map(TSource source);
    }
}