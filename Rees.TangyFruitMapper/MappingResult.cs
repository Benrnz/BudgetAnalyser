using System;
using System.Collections.Generic;

namespace Rees.TangyFruitMapper
{
    internal class MapResult
    {
        public IEnumerable<MapResult> DependentOnMaps { get; set; }

        /// <summary>
        ///     Gets a map with the Dto Property name as the key and the property name from the Model as the value.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentStrategy> DtoToModelMap { get; set; }

        public Type DtoType { get; set; }

        public string MapperName => GetMapperName(DtoType, ModelType);

        public ConstructionStrategy ModelConstructor { get; set; }

        /// <summary>
        ///     Gets a map with the Model Property name as the key and the property name from the Dto as the value.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentStrategy> ModelToDtoMap { get; set; }

        public Type ModelType { get; set; }

        public static string GetMapperName(Type dtoType, Type modelType)
        {
            return $"Mapper_{dtoType.Name}_{modelType.Name}";
        }

        /// <summary>
        ///     Makes sure that there are no inconsistencies between the Destination Assignment Strategy and Fetch Source Strategy.
        ///     For example: If the fetch source strategy is a commented out strategy, then the destination must also be commented
        ///     out.
        /// </summary>
        internal MapResult Consolidate()
        {
            EnsureBothStrategiesAreCommentedIfOneIsPresent(DtoToModelMap.Values);
            EnsureBothStrategiesAreCommentedIfOneIsPresent(ModelToDtoMap.Values);
            return this;
        }

        private static void EnsureBothStrategiesAreCommentedIfOneIsPresent(IEnumerable<AssignmentStrategy> strategies)
        {
            foreach (var strategy in strategies)
            {
                if (strategy.Source is CommentedFetchSource && !(strategy.Destination is CommentedAssignment))
                {
                    strategy.Destination = new CommentedAssignment(strategy.Destination.AssignmentDestinationName);
                }
                else if (strategy.Destination is CommentedAssignment && !(strategy.Source is CommentedFetchSource))
                {
                    strategy.Source = new CommentedFetchSource(strategy.Source.SourceName);
                }
            }
        }
    }
}