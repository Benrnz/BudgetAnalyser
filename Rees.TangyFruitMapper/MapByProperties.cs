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

            // DTO properties must be writable so not checked here.
            foreach (var dtoProperty in this.dtoType.GetProperties().Where(p => p.CanWrite && p.SetMethod.IsPublic))
            {
                this.diagnosticLogger($"Looking for a match for Dto property '{this.dtoType.Name}.{dtoProperty.Name}'");
                var assignmentStrategy = new AssignmentStrategy
                {
                    Destination = new SimpleAssignment // Dto Properties must be writeable so SimpleAssignment will always work.
                    {
                        AssignmentDestinationName = dtoProperty.Name,
                    }
                };
                this.dtoToModelMap[dtoProperty.Name] = assignmentStrategy;

                // Find a way to get the Source value...
                assignmentStrategy.Source = DoesSourceHavePropertyWithSameName(dtoProperty.Name, this.modelType);
                if (assignmentStrategy.Source != null)
                {
                    continue;
                }
                assignmentStrategy.Source = DoesSourceHaveFieldWithSimilarName(dtoProperty.Name, this.modelType);
                if (assignmentStrategy.Source != null)
                {
                    continue;
                }

                assignmentStrategy.Source = new CommentedFetchSource
                {
                    SourceName = dtoProperty.Name,
                };
            }

            foreach (var modelProperty in this.modelType.GetProperties())
            {
                this.diagnosticLogger($"Looking for a match for Model property '{this.modelType.Name}.{modelProperty.Name}'");
                var assignmentStrategy = new AssignmentStrategy();
                this.modelToDtoMap.Add(modelProperty.Name, assignmentStrategy);

                // Figure out how to assign into the destination property...
                assignmentStrategy.Destination = CanDestinationBeAssignedUsingAProperty(modelProperty);
                if (assignmentStrategy.Destination == null)
                {
                    assignmentStrategy.Destination = CanDestinationBeAssignedUsingReflection(modelProperty, this.modelType);
                    if (assignmentStrategy.Destination == null)
                    {
                        assignmentStrategy.Destination = new CommentedAssignment("TODO destination isn't writeable", modelProperty.Name);
                    }
                }

                // Find a way to get the Source value...
                assignmentStrategy.Source = DoesSourceHavePropertyWithSameName(modelProperty.Name, this.dtoType);
                if (assignmentStrategy.Source != null)
                {
                    continue;
                }
            }

            OutConditions();
        }

        private AssignDestinationStrategy CanDestinationBeAssignedUsingReflection(PropertyInfo modelProperty, Type destinationType)
        {
            if (modelProperty.CanWrite && modelProperty.SetMethod.IsPrivate)
            {
                return new PrivatePropertyAssignment(modelProperty.Name);
            }

            var field = FindSimilarlyNamedField(modelProperty.Name, destinationType);
            if (field != null)
            {
                return new PrivateFieldAssignment(field);
            }

            return null;
        }

        private AssignDestinationStrategy CanDestinationBeAssignedUsingAProperty(PropertyInfo modelProperty)
        {
            if (modelProperty.CanWrite && modelProperty.SetMethod.IsPublic)
            {
                return new SimpleAssignment
                {
                    AssignmentDestinationName = modelProperty.Name,
                };
            }

            return null;
        }

        private FetchSourceStrategy DoesSourceHaveFieldWithSimilarName(string targetPropertyName, Type assignmentSource)
        {
            // Looking for a backing field with a similar name
            var sourceField = FindSimilarlyNamedField(targetPropertyName, assignmentSource);
            if (sourceField != null)
            {
                return new FetchSourceByReflection(sourceField);
            }

            return null;
        }

        private static FieldInfo FindSimilarlyNamedField(string targetPropertyName, Type searchTarget)
        {
            var sourceField = searchTarget.GetField(targetPropertyName.ToLower());
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField(targetPropertyName);
            }
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField($"_{targetPropertyName.ToLower()}");
            }
            return sourceField;
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

        private FetchSourceStrategy DoesSourceHavePropertyWithSameName(string destinationName, Type assignmentSource)
        {
            var sourceProperty = FindMatchingSourceProperty(destinationName, assignmentSource);
            if (sourceProperty != null)
            {
                return new FetchSourceUsingPropertyAccess(sourceProperty.Name);
            }

            return null;
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
            DtoPropertiesMustBePublicAndWriteable();
        }

        private void DtoPropertiesMustBePublicAndWriteable()
        {
            int counter = 0;
            foreach (var property in this.dtoType.GetProperties(BindingFlags.Public))
            {
                counter++;
                if (!property.CanWrite || !property.SetMethod.IsPublic)
                {
                    throw new PropertiesMustBePublicWriteableException();
                }
            }

            this.diagnosticLogger($"{counter} public and writeable Dto properties found.");
        }
    }
}