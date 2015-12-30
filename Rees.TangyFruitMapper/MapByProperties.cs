using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Rees.TangyFruitMapper.Validation;

namespace Rees.TangyFruitMapper
{
    internal class MapByProperties
    {
        /// <summary>
        ///     All the maps. Keyed by mapper name.
        /// </summary>
        private static readonly ConcurrentDictionary<string, MapResult> AllMaps = new ConcurrentDictionary<string, MapResult>();

        private readonly List<MapResult> dependentMappers = new List<MapResult>();
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

        public IEnumerable<string> Warnings => this.warnings;

        public static void ClearMapCache()
        {
            AllMaps.Clear();
        }

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
                DependentOnMaps = this.dependentMappers
            }.Consolidate();
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

                if (assignmentStrategy.Source.SourceType.IsCollection())
                {
                    // Collection detected
                    MapCollection(assignmentStrategy, this.modelType, false);
                    continue;
                }

                if (assignmentStrategy.Source.SourceType.IsComplexType())
                {
                    // Nest objects detected - will need to attempt to map these as well.
                    this.diagnosticLogger($"Nested object graph detected on model property: {this.modelType.Name}.{assignmentStrategy.Source.SourceName}");
                    MapNestedObject(
                        assignmentStrategy,
                        this.modelType,
                        assignmentStrategy.Destination.DestinationType,
                        assignmentStrategy.Source.SourceType);
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

                if (assignmentStrategy.Source.SourceType.IsCollection())
                {
                    MapCollection(assignmentStrategy, this.dtoType, true);
                    continue;
                }

                if (assignmentStrategy.Source.SourceType.IsComplexType())
                {
                    this.diagnosticLogger($"Nested object graph detected on model property: {this.dtoType.Name}.{assignmentStrategy.Source.SourceName}");
                    MapNestedObject(
                        assignmentStrategy, 
                        this.dtoType, 
                        assignmentStrategy.Source.SourceType, 
                        assignmentStrategy.Destination.DestinationType);
                }
            }
        }

        private void MapNestedObject(AssignmentStrategy assignmentStrategy, Type parentType, Type dto, Type model)
        {
// Nest objects detected - will need to attempt to map these as well.
            var dependentMapper = AllMaps.GetOrAdd(
                MapResult.GetMapperName(dto, model),
                key =>
                {
                    var newMapper = new MapByProperties(
                        msg => this.diagnosticLogger($"->{parentType.Name} " + msg),
                        dto,
                        model)
                        .CreateMap();
                    this.dependentMappers.Add(newMapper);
                    return newMapper;
                });
            assignmentStrategy.Source = new FetchSourceAndCallMapper(assignmentStrategy.Source, dependentMapper);
        }

        private void MapCollection(AssignmentStrategy assignmentStrategy, Type parentType, bool sourceIsDto)
        {
// Collection detected
            this.diagnosticLogger($"Collection detected: {parentType.Name}.{assignmentStrategy.Source.SourceType}");
            if (assignmentStrategy.Source.SourceType.GetGenericArguments().Length != 1
                || assignmentStrategy.Destination.DestinationType.GetGenericArguments().Length != 1)
            {
                // Either the source or destination property types are not generic collections with one generic argument.
                assignmentStrategy.Source = new CommentedFetchSource(assignmentStrategy.Source.SourceName, "Either the source or destination property types are not generic collections with one generic argument.");
                assignmentStrategy.Destination = new CommentedAssignment(assignmentStrategy.Destination.AssignmentDestinationName, "Either the source or destination property types are not generic collections with one generic argument");
                return;
            }

            var genericSourceType = assignmentStrategy.Source.SourceType.GetGenericArguments()[0]; // Should be safe given the PreConditions
            var genericDestinationType = assignmentStrategy.Destination.DestinationType.GetGenericArguments()[0];
            var dtoGenericType = sourceIsDto ? genericSourceType : genericDestinationType;
            var modelGenericType = sourceIsDto ? genericDestinationType : genericSourceType;

            if (dtoGenericType.IsComplexType() && modelGenericType.IsComplexType())
            {
                // Both dto and model are complex types
                var dependentGenericTypeMapper = AllMaps.GetOrAdd(
                    MapResult.GetMapperName(dtoGenericType, modelGenericType),
                    key =>
                    {
                        var newMapper = new MapByProperties(
                            msg => this.diagnosticLogger($"->{parentType.Name} " + msg),
                            dtoGenericType,
                            modelGenericType)
                            .CreateMap();
                        this.dependentMappers.Add(newMapper);
                        return newMapper;
                    });
                assignmentStrategy.Source = new FetchSourceAndMapList(assignmentStrategy.Source, dependentGenericTypeMapper);
                return;
            }

            if (!dtoGenericType.IsComplexType() && !modelGenericType.IsComplexType())
            {
                // Both dto and model are simple types
                assignmentStrategy.Source = new FetchSourceList(assignmentStrategy.Source);
                return;
            }

            // The dto and model types appear to be incompatible types of list
            assignmentStrategy.Source = new CommentedFetchSource(assignmentStrategy.Source.SourceName, "Either the source or destination property types is a complex type and the other is a simple type.");
            assignmentStrategy.Destination = new CommentedAssignment(assignmentStrategy.Destination.AssignmentDestinationName, "Either the source or destination property types is a complex type and the other is a simple type.");
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

        private void MustHaveADefaultConstructor()
        {
            var modelCtor = this.modelType.GetConstructor(new Type[] {});
            if (modelCtor == null)
            {
                throw new NoAccessibleDefaultConstructorException($"No constructor found on {this.modelType.Name}");
            }

            var dtoCtor = this.dtoType.GetConstructor(new Type[] {});
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
            VisitAllProperties(
                this.dtoType,
                new ConcurrentDictionary<Type, object>(),
                new DictionariesAreNotSupportedRule(),
                new MustOnlyHavePublicWriteablePropertiesRule(), 
                new MustOnlyUseListForCollectionsRule());
            VisitAllProperties(
                this.modelType,
                new ConcurrentDictionary<Type, object>(),
                new DictionariesAreNotSupportedRule());
        }

        private void VisitAllProperties(Type type, ConcurrentDictionary<Type, object> typeCheckList, params PreconditionRule[] rules)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var rule in rules)
                {
                    rule.IsCompliant(property);
                }
                typeCheckList.GetOrAdd(type, key => null);
                if (property.PropertyType.IsComplexType() && !property.PropertyType.IsCollection())
                {
                    typeCheckList.GetOrAdd(property.PropertyType, key => null);
                    VisitAllProperties(property.PropertyType, typeCheckList, rules);
                }
            }
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