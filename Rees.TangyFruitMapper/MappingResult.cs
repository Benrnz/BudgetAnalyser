using System;
using System.Collections.Generic;

namespace Rees.TangyFruitMapper
{
    internal class MapResult
    {
        /// <summary>
        ///     Gets a map with the Dto Property name as the key and the property name from the Model as the value.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentStrategy> DtoToModelMap { get; set; }

        /// <summary>
        ///     Gets a map with the Model Property name as the key and the property name from the Dto as the value.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentStrategy> ModelToDtoMap { get; set; }

        public Type DtoType { get; set; }

        public Type ModelType { get; set; }

        public IEnumerable<MapResult> DependentOnMaps { get; set; } 
    }
}
