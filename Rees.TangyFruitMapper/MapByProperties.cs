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
        private readonly Dictionary<string, AssignmentStrategy> dtoToModelMap = new Dictionary<string, AssignmentStrategy>();
        private readonly Type dtoType;
        private readonly Dictionary<string, AssignmentStrategy> modelToDtoMap = new Dictionary<string, AssignmentStrategy>();
        private readonly Type modelType;
        private readonly List<string> warnings = new List<string>();
        private readonly List<MapResult> dependentMappers = new List<MapResult>();

        public MapByProperties([NotNull] Action<string> diagnosticLogger, [NotNull] Type dtoType, [NotNull] Type modelType)
        {
            if (diagnosticLogger == null) throw new ArgumentNullException(nameof(diagnosticLogger));
            if (dtoType == null) throw new ArgumentNullException(nameof(dtoType));
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            this.diagnosticLogger = diagnosticLogger;
            this.dtoType = dtoType;
            this.modelType = modelType;
        }

        public IEnumerable<string> Warnings => this.warnings;

        public MapResult CreateMap()
        {
            Preconditions();

            CreateMapToDto();

            CreateMapToModel();

            OutConditions();

            return new MapResult
            {
                DtoType = this.dtoType,
                ModelType = this.modelType,
                DtoToModelMap = this.dtoToModelMap,
                ModelToDtoMap = this.modelToDtoMap,
                DependentOnMaps = this.dependentMappers,
            };
        }

        private AssignDestinationStrategy CanDestinationBeAssignedUsingAProperty(PropertyInfo modelProperty)
        {
            if (modelProperty.CanWrite && modelProperty.SetMethod.IsPublic)
            {
                return new SimpleAssignment(modelProperty.PropertyType)
                {
                    AssignmentDestinationName = modelProperty.Name
                };
            }

            return null;
        }

        private AssignDestinationStrategy CanDestinationBeAssignedUsingReflection(PropertyInfo modelProperty, Type destinationType)
        {
            if (modelProperty.CanWrite && modelProperty.SetMethod.IsPrivate)
            {
                return new PrivatePropertyAssignment(modelProperty.Name, modelProperty.PropertyType);
            }

            var field = FindSimilarlyNamedField(modelProperty.Name, destinationType);
            if (field != null)
            {
                return new PrivateFieldAssignment(field);
            }

            return null;
        }

        private void CreateMapToDto()
        {
            // DTO properties must be writable so not checked here.
            foreach (var dtoProperty in this.dtoType.GetProperties().Where(p => p.CanWrite && p.SetMethod.IsPublic))
            {
                this.diagnosticLogger($"Looking for a match for Dto property '{this.dtoType.Name}.{dtoProperty.Name}'");
                var assignmentStrategy = new AssignmentStrategy
                {
                    Destination = new SimpleAssignment(dtoProperty.PropertyType) // Dto Properties must be writeable so SimpleAssignment will always work.
                    {
                        AssignmentDestinationName = dtoProperty.Name
                    }
                };
                this.dtoToModelMap[dtoProperty.Name] = assignmentStrategy;

                // Find a way to get the Source value...
                assignmentStrategy.Source = DoesSourceHavePropertyWithSameName(dtoProperty.Name, this.modelType);
                if (assignmentStrategy.Source == null)
                {
                    assignmentStrategy.Source = DoesSourceHaveFieldWithSimilarName(dtoProperty.Name, this.modelType);
                    if (assignmentStrategy.Source == null)
                    {
                        assignmentStrategy.Source = new CommentedFetchSource(dtoProperty.Name);
                    }
                }

                if (IsComplexType(assignmentStrategy.Source.SourceType))
                {
                    // Nest objects detected - will need to attempt to map these as well.
                    this.diagnosticLogger($"Nested object graph detected on model property: {this.modelType.Name}.{assignmentStrategy.Source.SourceName}");
                    // TODO this could result in duplicate maps being created.
                    var dependentMapper = new MapByProperties(
                        msg => this.diagnosticLogger($"->{this.modelType.Name} " + msg),
                        assignmentStrategy.Destination.DestinationType,
                        assignmentStrategy.Source.SourceType)
                        .CreateMap();
                    this.dependentMappers.Add(dependentMapper);
                    assignmentStrategy.Source = new FetchSourceAndMap(assignmentStrategy.Source, dependentMapper);
                }
            }
        }

        private void CreateMapToModel()
        {
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
                        assignmentStrategy.Destination = new CommentedAssignment(modelProperty.Name);
                    }
                }

                // Find a way to get the Source value...
                assignmentStrategy.Source = DoesSourceHavePropertyWithSameName(modelProperty.Name, this.dtoType);
                if (assignmentStrategy.Source == null)
                {
                    assignmentStrategy.Source = DoesSourceHaveFieldWithSimilarName(modelProperty.Name, this.dtoType);
                    if (assignmentStrategy.Source == null)
                    {
                        assignmentStrategy.Source = new CommentedFetchSource(modelProperty.Name);
                    }
                }

                if (IsComplexType(assignmentStrategy.Source.SourceType))
                {
                    // Nest objects detected - will need to attempt to map these as well.
                    this.diagnosticLogger($"Nested object graph detected on model property: {this.modelType.Name}.{assignmentStrategy.Source.SourceName}");
                    // TODO this could result in duplicate maps being created.
                    var dependentMapper = new MapByProperties(
                        msg => this.diagnosticLogger($"->{this.modelType.Name} " + msg),
                        assignmentStrategy.Source.SourceType,
                        assignmentStrategy.Destination.DestinationType)
                        .CreateMap();
                    this.dependentMappers.Add(dependentMapper);
                    assignmentStrategy.Source = new FetchSourceAndMap(assignmentStrategy.Source, dependentMapper);
                }
            }
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

        private FetchSourceStrategy DoesSourceHavePropertyWithSameName(string destinationName, Type assignmentSource)
        {
            var sourceProperty = FindMatchingSourceProperty(destinationName, assignmentSource);
            if (sourceProperty != null)
            {
                return new FetchSourceUsingPropertyAccess(sourceProperty.Name, sourceProperty.PropertyType);
            }

            return null;
        }

        private void DtoPropertiesMustBePublicAndWriteable()
        {
            var counter = 0;
            foreach (var property in this.dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                counter++;
                if (!property.CanWrite || !property.SetMethod.IsPublic)
                {
                    throw new PropertiesMustBePublicWriteableException();
                }
            }

            this.diagnosticLogger($"{counter} public and writeable Dto properties found.");
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

        private static FieldInfo FindSimilarlyNamedField(string targetPropertyName, Type searchTarget)
        {
            var sourceField = searchTarget.GetField(targetPropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField(targetPropertyName.ToLower(), BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField(targetPropertyName.ConvertPascalCaseToCamelCase(), BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField($"_{targetPropertyName.ToLower()}", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (sourceField == null)
            {
                sourceField = searchTarget.GetField($"_{targetPropertyName.ConvertPascalCaseToCamelCase()}", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return sourceField;
        }

        private bool IsComplexType(Type sourceType)
        {
            if (sourceType.GetTypeInfo().IsPrimitive)
            {
                // https://msdn.microsoft.com/en-us/library/system.type.isprimitive(v=vs.110).aspx
                // Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
                return false;
            }

            if (sourceType == typeof (decimal) || sourceType == typeof(string))
            {
                return false;
            }

            return true;
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

        private void OutConditions()
        {
            // Check that all available Dto properties have been mapped to a model property.
            WarnIfMissingProperties(this.dtoType, this.dtoToModelMap);
            WarnIfMissingProperties(this.modelType, this.modelToDtoMap);
        }

        private void Preconditions()
        {
            MustHaveADefaultConstructor();
            DtoPropertiesMustBePublicAndWriteable();
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
    }
}