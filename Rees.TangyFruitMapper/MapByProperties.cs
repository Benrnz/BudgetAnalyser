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

        private static int RecursionFailSafe;

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
            this.diagnosticLogger("MapByProperties mapper-generator created.");
        }

        public IEnumerable<string> Warnings => this.warnings;

        /// <summary>
        ///     Clear the static cache used to ensure the same type mapping isn't done twice.
        /// </summary>
        public static void ClearMapCache()
        {
            AllMaps.Clear();
        }

        public MapResult CreateMap(bool skipPreconditions = false)
        {
            this.diagnosticLogger($"CreateMap for mapping {this.dtoType.FullName} to {this.modelType.FullName}");
            this.diagnosticLogger($"Recursion Index: {RecursionFailSafe++}");
            if (RecursionFailSafe > 100)
            {
                throw new CodeGenerationFailedException("Too many nested objects or cyclic depedency detected. Aborting to avoid StackOverflow.");
            }
            if (skipPreconditions)
            {
                this.diagnosticLogger($"Skipping Preconditions.");
            }
            else
            {
                Preconditions();
            }

            CreateMapToDto();

            CreateMapToModel();

            OutConditions();

            return new MapResult
            {
                ModelConstructor = new ConstructionStrategyBuilder().Build(this.modelType),
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
            if (modelProperty.CanWrite && !modelProperty.SetMethod.IsPublic)
            {
                return new PrivatePropertyAssignment(modelProperty.Name, modelProperty.PropertyType);
            }

            // Note this code is unreachable at this point.  Only properties with setters are mapped at this stage.
            var field = FindSimilarlyNamedField(modelProperty.Name, destinationType);
            if (field != null)
            {
                return new PrivateFieldAssignment(field);
            }

            return null;
        }

        private void CreateMapToDto()
        {
            this.diagnosticLogger($"Creating a map to Map To a Dto {this.dtoType.FullName}.");
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
                this.diagnosticLogger($"SimpleAssignment will be used to assign a value into {this.dtoType.Name}.{dtoProperty.Name}");

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

                this.diagnosticLogger($"First pass to get source value for {this.dtoType.Name}.{dtoProperty.Name} from {this.modelType.Name} complete, using {assignmentStrategy.Source.GetType().Name}");

                if (assignmentStrategy.Source.SourceType.IsCollection())
                {
                    // Collection detected
                    MapCollection(assignmentStrategy, this.modelType, false);
                    continue;
                }

                if (assignmentStrategy.Source.SourceType.IsComplexType())
                {
                    // Nest objects detected - will need to attempt to map these as well.
                    MapNestedObject(assignmentStrategy, this.modelType, false);
                }

                this.diagnosticLogger(
                    $"Second pass to get source value for {this.dtoType.Name}.{dtoProperty.Name} from {this.modelType.Name} complete, will use {assignmentStrategy.Source.GetType().Name}");
            }
        }

        private void CreateMapToModel()
        {
            this.diagnosticLogger($"Creating a map to Map To a Model {this.modelType.FullName}.");
            foreach (var modelProperty in this.modelType.GetProperties().Where(p => p.CanWrite))
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

                this.diagnosticLogger($"{assignmentStrategy.Destination.GetType()} will be used to assign a value into {this.modelType.Name}.{modelProperty.Name}.");

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

                this.diagnosticLogger(
                    $"First pass to get source value for {this.modelType.Name}.{modelProperty.Name} from {this.dtoType.Name} complete, using {assignmentStrategy.Source.GetType().Name}");

                if (assignmentStrategy.Source.SourceType.IsCollection())
                {
                    MapCollection(assignmentStrategy, this.dtoType, true);
                    continue;
                }

                if (assignmentStrategy.Source.SourceType.IsComplexType())
                {
                    MapNestedObject(assignmentStrategy, this.dtoType, true);
                }

                this.diagnosticLogger(
                    $"Second pass to get source value for {this.modelType.Name}.{modelProperty.Name} from {this.dtoType.Name} complete, will use {assignmentStrategy.Source.GetType().Name}");
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

        private void MapCollection(AssignmentStrategy assignmentStrategy, Type parentType, bool sourceIsDto)
        {
// Collection detected
            this.diagnosticLogger($"Collection detected: {parentType.Name}.{assignmentStrategy.Source.SourceType}. Examining collection...");
            if (assignmentStrategy.Source.SourceType.GetGenericArguments().Length != 1
                || assignmentStrategy.Destination.DestinationType.GetGenericArguments().Length != 1)
            {
                this.diagnosticLogger($"The source and/or the destination are not generic collections. Commenting out the assignment.");
                // Either the source or destination property types are not generic collections with one generic argument.
                assignmentStrategy.Source = new CommentedFetchSource(assignmentStrategy.Source.SourceName,
                    "Either the source or destination property types are not generic collections with one generic argument.");
                assignmentStrategy.Destination = new CommentedAssignment(assignmentStrategy.Destination.AssignmentDestinationName,
                    "Either the source or destination property types are not generic collections with one generic argument");
                return;
            }

            var genericSourceType = assignmentStrategy.Source.SourceType.GetGenericArguments()[0]; // Should be safe given the PreConditions
            var genericDestinationType = assignmentStrategy.Destination.DestinationType.GetGenericArguments()[0];
            var dtoGenericType = sourceIsDto ? genericSourceType : genericDestinationType;
            var modelGenericType = sourceIsDto ? genericDestinationType : genericSourceType;

            if (dtoGenericType.IsComplexType() && modelGenericType.IsComplexType())
            {
                this.diagnosticLogger($"Both source List<{genericSourceType}> and destination List<{genericDestinationType}> are generic collections with a complex generic argument.");
                // Both dto and model are complex types
                var dependentGenericTypeMapper = AllMaps.GetOrAdd(
                    MapResult.GetMapperName(dtoGenericType, modelGenericType),
                    key =>
                    {
                        var newMapper = new MapByProperties(
                            msg => this.diagnosticLogger($"->{parentType.Name} " + msg),
                            dtoGenericType,
                            modelGenericType)
                            .CreateMap(true);
                        this.dependentMappers.Add(newMapper);
                        return newMapper;
                    });
                assignmentStrategy.Source = new FetchSourceAndMapList(assignmentStrategy.Source, dependentGenericTypeMapper);
                this.diagnosticLogger($"Mapping collection generic argument complete for {parentType.Name}.{assignmentStrategy.Source.SourceType}");
                this.diagnosticLogger($"    Can now map from List<{genericSourceType}> to List<{genericDestinationType}>");
                return;
            }

            if (!dtoGenericType.IsComplexType() && !modelGenericType.IsComplexType())
            {
                // Both dto and model are simple types
                this.diagnosticLogger($"Both source List<{genericSourceType}> and destination List<{genericDestinationType}> are generic collections with a simple generic argument.");
                assignmentStrategy.Source = new FetchSourceList(assignmentStrategy.Source);
                return;
            }

            // The dto and model types appear to be incompatible types of list
            assignmentStrategy.Source = new CommentedFetchSource(assignmentStrategy.Source.SourceName,
                "Either the source or destination property types is a complex type and the other is a simple type.");
            assignmentStrategy.Destination = new CommentedAssignment(assignmentStrategy.Destination.AssignmentDestinationName,
                "Either the source or destination property types is a complex type and the other is a simple type.");
        }

        private void MapNestedObject(AssignmentStrategy assignmentStrategy, Type parentType, bool sourceIsDto)
        {
            // Nest objects detected - will need to attempt to map these as well.
            this.diagnosticLogger($"Nested object graph detected on model property: {parentType.Name}.{assignmentStrategy.Source.SourceName}");
            var nestedDtoType = sourceIsDto ? assignmentStrategy.Source.SourceType : assignmentStrategy.Destination.DestinationType;
            var nestedModelType = sourceIsDto ? assignmentStrategy.Destination.DestinationType : assignmentStrategy.Source.SourceType;
            var dependentMapper = AllMaps.GetOrAdd(
                MapResult.GetMapperName(nestedDtoType, nestedModelType),
                key =>
                {
                    var newMapper = new MapByProperties(
                        msg => this.diagnosticLogger($"->{parentType.Name} " + msg),
                        nestedDtoType,
                        nestedModelType)
                        .CreateMap(true);
                    this.dependentMappers.Add(newMapper);
                    return newMapper;
                });
            assignmentStrategy.Source = new FetchSourceAndCallMapper(assignmentStrategy.Source, dependentMapper);
            this.diagnosticLogger($"Mapping nested object complete for {parentType.Name}.{assignmentStrategy.Source.SourceName}");
        }

        private void OutConditions()
        {
            // Check that all available Dto properties have been mapped to a model property.
            WarnIfMissingProperties(this.dtoType, this.dtoToModelMap);
            WarnIfMissingProperties(this.modelType, this.modelToDtoMap);
        }

        private void Preconditions()
        {
            this.diagnosticLogger("Evaluating Preconditions (Exceptions will be thrown in Preconditions are not met)...");
            VisitType(this.dtoType, new HasAccessibleConstructorRule());
            VisitType(this.modelType, new HasAccessibleConstructorRule());
            this.diagnosticLogger("Constructors meet convention requirements.");
            this.diagnosticLogger($"Analysing all properties recursively for {this.dtoType.FullName}");
            var recursionFailSafe = 0;
            VisitAllProperties(
                recursionFailSafe,
                this.dtoType,
                new ConcurrentDictionary<Type, object>(),
                new DictionariesAreNotSupportedRule(),
                new MustOnlyHavePublicWriteablePropertiesRule(),
                new MustOnlyUseListForCollectionsRule());
            this.diagnosticLogger($"{this.dtoType.FullName} meets Precondition requirements.");
            this.diagnosticLogger($"Analysing all properties recursively for {this.modelType.FullName}");
            recursionFailSafe = 0;
            VisitAllProperties(
                recursionFailSafe,
                this.modelType,
                new ConcurrentDictionary<Type, object>(),
                new DictionariesAreNotSupportedRule());
            this.diagnosticLogger($"{this.modelType.FullName} meets Precondition requirements.");
        }

        private void VisitAllProperties(int recursionIndex, Type typeToCheck, ConcurrentDictionary<Type, object> typeCheckList, params PreconditionPropertyRule[] rules)
        {
            if (recursionIndex++ > 100)
            {
                throw new CodeGenerationFailedException("Unable to apply Precondition rules. Cyclic depedencies detected or too many nested types. Aborting to avoid StackOverflow.");
            }
            foreach (var property in typeToCheck.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var rule in rules)
                {
                    rule.IsCompliant(property);
                }
                typeCheckList.GetOrAdd(typeToCheck, key => null);
                if (property.PropertyType.IsComplexType() && !property.PropertyType.IsCollection())
                {
                    typeCheckList.GetOrAdd(property.PropertyType, key => null);
                    VisitAllProperties(recursionIndex, property.PropertyType, typeCheckList, rules);
                }
            }
        }

        private void VisitType(Type typeToCheck, params PreconditionTypeRule[] rules)
        {
            foreach (var rule in rules)
            {
                rule.IsCompliant(typeToCheck);
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