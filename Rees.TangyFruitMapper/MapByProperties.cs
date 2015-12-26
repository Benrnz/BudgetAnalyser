using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    internal class MapByProperties
    {
        private readonly Action<string> diagnosticLogger;
        private readonly Dictionary<string, AssignmentStrategy> dtoToModelMap = new Dictionary<string, AssignmentStrategy>();
        private readonly Type dtoType;
        private readonly Dictionary<string, AssignmentStrategy> modelToDtoMap = new Dictionary<string, AssignmentStrategy>();
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
        public IReadOnlyDictionary<string, AssignmentStrategy> DtoToModelMap => this.dtoToModelMap;

        /// <summary>
        /// Gets a map with the Model Property name as the key and the property name from the Dto as the value.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentStrategy> ModelToDtoMap => this.modelToDtoMap;

        public IEnumerable<string> Warnings => this.warnings;

        public void CreateMap()
        {
            Preconditions();
            // DTO properties must be writable
            foreach (var dtoProperty in this.dtoType.GetProperties().Where(p => p.CanWrite && p.SetMethod.IsPublic))
            {
                this.diagnosticLogger($"Looking for a match for Dto property '{this.dtoType.Name}.{dtoProperty.Name}'");
                this.dtoToModelMap.Add(dtoProperty.Name, new CommentedAssignment("TODO value not found on model.")
                {
                    AssignmentDestination = dtoProperty.Name,
                    AssignmentDestinationIsDto = true,
                });
                if (AttemptMapToSourceProperty(dtoProperty.Name, this.modelType, this.dtoToModelMap)) continue;
                if (AttemptMapToSourceField(dtoProperty.Name, this.dtoType, this.modelType, this.dtoToModelMap)) continue;
            }

            foreach (var modelProperty in this.modelType.GetProperties())
            {
                this.diagnosticLogger($"Looking for a match for property '{this.modelType.Name}.{modelProperty.Name}'");
                this.modelToDtoMap.Add(modelProperty.Name, new CommentedAssignment("TODO value not found on model.")
                {
                    AssignmentDestination = modelProperty.Name,
                    AssignmentDestinationIsDto = false,
                });
                if (AttemptMapToSourceProperty(modelProperty.Name, this.dtoType, this.modelToDtoMap)) continue;
                // Attempt Field map 
                
            }

            OutConditions();
        }

        private bool AttemptMapToSourcePrivateSetter(PropertyInfo assignmentDestination, Type assignmentSource, Dictionary<string, AssignmentStrategy> mapping)
        {
            var property = assignmentSource.GetProperty(assignmentDestination.Name);
            if (property == null) return false;
            var setterMethod = property.SetMethod;

        }

        private bool AttemptMapToSourceField(string destinationName, Type destinationType, Type assignmentSource, Dictionary<string, AssignmentStrategy> mapping)
        {
            // Looking for a backing field with a similar name
            var sourceField = assignmentSource.GetField(destinationName.ToLower());
            if (sourceField == null)
            {
                sourceField = assignmentSource.GetField(destinationName);
            }
            if (sourceField == null)
            {
                sourceField = assignmentSource.GetField($"_{destinationName.ToLower()}");
            }

            if (sourceField != null)
            {
                mapping[destinationName] = new PrivateFieldAssignment(destinationType, sourceField)
                {
                    AssignmentDestination = destinationName,
                    AssignmentDestinationIsDto = mapping[destinationName].AssignmentDestinationIsDto,
                };
                return true;
            }

            return false;
        }

        private void OutConditions()
        {
            // Check that all available Dto properties have been mapped to a model property.
            WarnIfMissingProperties(this.dtoType, this.dtoToModelMap);
            WarnIfMissingProperties(this.modelType, this.modelToDtoMap);
        }

        private void WarnIfMissingProperties(Type target, Dictionary<string, AssignmentStrategy> map)
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

        private bool AttemptMapToSourceProperty(string destinationName, Type assignmentSource, Dictionary<string, AssignmentStrategy> mapping)
        {
            var sourceProperty = FindMatchingSourceProperty(destinationName, assignmentSource);
            if (sourceProperty != null)
            {
                mapping[destinationName] = new SimpleAssignment
                {
                    AssignmentDestination = destinationName,
                    AssignmentDestinationIsDto = mapping[destinationName].AssignmentDestinationIsDto,
                    AssignmentSource = sourceProperty.Name,
                };
                return true;
            }

            return false;
        }

        private PropertyInfo FindMatchingSourceProperty(string destinationName, Type source)
        {
            var sourceProperty = source.GetProperty(destinationName);
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
                this.warnings.Add($"    WARNING: No Source property found to map to: {destinationName} - it will be ignored.");
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