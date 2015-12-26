using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    internal class MapByProperties
    {
        private readonly Action<string> diagnosticLogger;
        private readonly Dictionary<string, string> dtoToModelMap = new Dictionary<string, string>();
        private readonly Type dtoType;
        private readonly Dictionary<string, string> modelToDtoMap = new Dictionary<string, string>();
        private readonly Type modelType;
        private readonly List<string> warnings = new List<string>();

        public MapByProperties([NotNull] Action<string> diagnosticLogger, [NotNull] Type dtoType, [NotNull] Type modelType)
        {
            if (diagnosticLogger == null) throw new ArgumentNullException(nameof(diagnosticLogger));
            if (dtoType == null) throw new ArgumentNullException(nameof(dtoType));
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            this.diagnosticLogger = diagnosticLogger;
            this.dtoType = dtoType;
            this.modelType = modelType;
        }

        /// <summary>
        /// Gets a map with the Dto Property name as the key and the property name from the Model as the value.
        /// </summary>
        public IReadOnlyDictionary<string, string> DtoToModelMap => this.dtoToModelMap;

        /// <summary>
        /// Gets a map with the Model Property name as the key and the property name from the Dto as the value.
        /// </summary>
        public IReadOnlyDictionary<string, string> ModelToDtoMap => this.modelToDtoMap;

        public IEnumerable<string> Warnings => this.warnings;

        public void CreateMap()
        {
            Preconditions();
            foreach (var dtoProperty in this.dtoType.GetProperties())
            {
                this.diagnosticLogger($"Looking for a match for property '{this.dtoType.Name}.{dtoProperty.Name}'");
                this.dtoToModelMap.Add(dtoProperty.Name, "// TODO value not found on model.");
                if (AttemptMapToProperty(dtoProperty, this.modelType, this.dtoToModelMap)) continue;
                // Attempt Field map 
            }

            foreach (var modelProperty in this.modelType.GetProperties())
            {
                this.diagnosticLogger($"Looking for a match for property '{this.modelType.Name}.{modelProperty.Name}'");
                this.modelToDtoMap.Add(modelProperty.Name, "// TODO value not found on model.");
                if (AttemptMapToProperty(modelProperty, this.dtoType, this.modelToDtoMap)) continue;
                // Attempt Field map 
            }

            OutConditions();
        }

        private void OutConditions()
        {
            // Check that all available Dto properties have been mapped to a model property.
            WarnIfMissingProperties(this.dtoType, this.dtoToModelMap);
            WarnIfMissingProperties(this.modelType, this.modelToDtoMap);
        }

        private void WarnIfMissingProperties(Type target, Dictionary<string, string> map)
        {
            var query = target.GetProperties().Where(p => p.CanWrite).ToList();
            if (map.Count < query.Count())
            {
                this.warnings.Add($"Warning: Not all properties on Type '{target.FullName}' can be found on source.");
                var missingProperties = query.Select(p => p.Name).Except(map.Keys);
                foreach (var missingProperty in missingProperties)
                {
                    this.warnings.Add($"    {missingProperty}");
                }
            }
        }

        private bool AttemptMapToProperty(PropertyInfo assignmentDestination, Type assignmentSource, Dictionary<string, string> mapping)
        {
            var sourceProperty = FindMatchingSourceProperty(assignmentDestination, assignmentSource);
            if (sourceProperty != null)
            {
                mapping[assignmentDestination.Name] = sourceProperty.Name;
                return true;
            }

            return false;
        }

        private PropertyInfo FindMatchingSourceProperty(PropertyInfo property, Type source)
        {
            var sourceProperty = source.GetProperty(property.Name);
            if (sourceProperty != null)
            {
                this.diagnosticLogger($"    Found match with same name on destination type.");
                if (sourceProperty.GetMethod.IsPublic)
                {
                    this.diagnosticLogger($"    Source property is public, all is good.");
                    return sourceProperty;
                }

                this.warnings.Add($"    WARNING: Source property isn't public: {sourceProperty.Name} - it will be ignored.");
            }
            else
            {
                this.warnings.Add($"    WARNING: No Source property found to map to: {property.DeclaringType.Name}.{property.Name} - it will be ignored.");
            }

            // Source property isn't public
            return null;
        }

        private void MustHaveADefaultConstructor()
        {
            var modelCtor = this.modelType.GetConstructor(new Type[] {});
            if (modelCtor == null)
            {
                throw new NoAccessibleDefaultConstructorException($"No constructor found on {this.modelType.Name}");
            }

            var dtoCtor = this.modelType.GetConstructor(new Type[] {});
            if (dtoCtor == null)
            {
                throw new NoAccessibleDefaultConstructorException($"No constructor found on {this.dtoType.Name}");
            }
        }

        private void Preconditions()
        {
            MustHaveADefaultConstructor();
        }
    }
}