using System;
using System.Linq;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    /// <summary>
    ///     A Convention based C# code generator for mapping a model object to a DTO object and back.
    ///     This class is designed to be used either with T4, console application, or a unit test.
    /// </summary>
    public class MappingGenerator
    {
        private Action<string> codeOutput;
        private Type dtoType;
        private int indent;
        private Type modelType;
        private NamespaceFinder namespaceFinder;

        /// <summary>
        ///     An optional delegate to a logging action to output diagnostic messages for debugging and troubleshooting purposes.
        /// </summary>
        public Action<string> DiagnosticLogging { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether generated code will be emitted as internal classes.
        /// </summary>
        public bool EmitWithInternalAccessors { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the generated code. Defaults to "GeneratedCode".
        /// </summary>
        public string Namespace { get; set; } = "GeneratedCode";

        /// <summary>
        ///     Generates the code for the specified types. Be sure to check for TODO's in the generated code.
        /// </summary>
        /// <typeparam name="TDto">The type of the dto. It is important that this Dto follows the Dto conventions.</typeparam>
        /// <typeparam name="TModel">The type of the model. There are less convention rules for model objects.</typeparam>
        /// <param name="codeOutputDelegate">An action to output the code.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Generate<TDto, TModel>([NotNull] Action<string> codeOutputDelegate)
        {
            if (codeOutputDelegate == null) throw new ArgumentNullException(nameof(codeOutputDelegate));
            if (DiagnosticLogging == null) DiagnosticLogging = x => { };

            this.codeOutput = codeOutputDelegate;
            this.modelType = typeof(TModel);
            this.dtoType = typeof(TDto);
            this.namespaceFinder = new NamespaceFinder(this.dtoType, this.modelType);
            DiagnosticLogging($"Starting to generate code for mapping {this.dtoType.Name} to {this.modelType.Name}...");

            MapByProperties.ClearMapCache();
            var mapper = new MapByProperties(DiagnosticLogging, this.dtoType, this.modelType);
            var mapResult = mapper.CreateMap();

            WriteFileHeader();

            WriteMappingClasses(mapResult);

            WriteFileFooter();

            DiagnosticLogging($"================== Mapping Complete {this.dtoType.Name} to {this.modelType.Name} ======================");
        }

        private string Indent(bool increment = false)
        {
            if (increment) this.indent++;
            return new string(' ', 4*this.indent);
        }

        private string Outdent()
        {
            this.indent--;
            if (this.indent < 0) this.indent = 0;
            return new string(' ', 4*this.indent);
        }

        private void WriteClassFooter()
        {
            this.codeOutput($@"{Outdent()}}} // End Class
");
        }

        private void WriteClassHeader(MapResult map)
        {
            var classAccessor = EmitWithInternalAccessors ? "internal" : "public";
            this.codeOutput(
                $@"{Indent()}[GeneratedCode(""1.0"", ""Tangy Fruit Mapper {DateTime.UtcNow} UTC"")]
{Indent()}{classAccessor} partial class {map.MapperName} : IDtoMapper<{map.DtoType.Name}, {map.ModelType.Name}>
{Indent()}{{
{
                    Indent(true)}");
        }

        private void WriteFileFooter()
        {
            this.codeOutput($@"{Outdent()}}} // End Namespace");
        }

        private void WriteFileHeader()
        {
            this.codeOutput($@"using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;");
            foreach (var ns in this.namespaceFinder.DiscoverNamespaces())
            {
                this.codeOutput($@"using {ns.Key};");
            }
            this.codeOutput($@"
namespace {Namespace}
{{
{Indent(true)}");
        }

        private void WriteMappingClasses(MapResult mapResult)
        {
            WriteClassHeader(mapResult);
            WriteMethods(mapResult);
            WriteClassFooter();

            if (mapResult.DependentOnMaps.Any())
            {
                foreach (var nestedMap in mapResult.DependentOnMaps)
                {
                    WriteMappingClasses(nestedMap);
                }
            }
        }

        private void WriteMethods(MapResult map)
        {
            // ToModel Method
            this.codeOutput($@"{Indent()}public virtual {map.ModelType.Name} ToModel({map.DtoType.Name} {AssignmentStrategy.DtoVariableName})
{Indent()}{{");

            // Construct Model
            this.codeOutput($@"{Indent(true)}{map.ModelType.Name} {AssignmentStrategy.ModelVariableName} = null;
{Indent()}ModelFactory({AssignmentStrategy.DtoVariableName}, ref {AssignmentStrategy.ModelVariableName});
{Indent()}if ({AssignmentStrategy.ModelVariableName} == null) 
{Indent()}{{
{Indent(true)}{map.ModelConstructor.CreateCodeLine(DtoOrModel.Model)}
{Outdent()}}}
{Indent()}var {AssignmentStrategy.ModelTypeVariableName} = {AssignmentStrategy.ModelVariableName}.GetType();");

            // Optional Pre-processing
            this.codeOutput($"{Indent()}ToModelPreprocessing({AssignmentStrategy.DtoVariableName}, {AssignmentStrategy.ModelVariableName});");

            // Assign properties by convention if possible
            foreach (var assignment in map.ModelToDtoMap.Values)
            {
                // model.Property = dto.Property;
                this.codeOutput($"{Indent()}{assignment.Source.CreateCodeLine(DtoOrModel.Dto)}");
                this.codeOutput($"{Indent()}{assignment.Destination.CreateCodeLine(DtoOrModel.Model, assignment.Source.SourceVariableName)}");
            }
            if (!map.ModelToDtoMap.Any())
            {
                this.codeOutput($"{Indent()} // TODO No properties found to map.");
            }

            // Optional Post-processing
            this.codeOutput($@"{Indent()}ToModelPostprocessing({AssignmentStrategy.DtoVariableName}, ref {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}return {AssignmentStrategy.ModelVariableName};
{Outdent()}}} // End ToModel Method");


            // ToDto Method
            this.codeOutput($@"{Indent()}public virtual {map.DtoType.Name} ToDto({map.ModelType.Name} {AssignmentStrategy.ModelVariableName})
{Indent()}{{");

            // Construct Dto
            this.codeOutput($@"{Indent(true)}{map.DtoType.Name} {AssignmentStrategy.DtoVariableName} = null;
{Indent()}DtoFactory(ref {AssignmentStrategy.DtoVariableName}, {AssignmentStrategy.ModelVariableName});
{Indent()}if ({AssignmentStrategy.DtoVariableName} == null) 
{Indent()}{{
{Indent(true)}{AssignmentStrategy.DtoVariableName} = new {map.DtoType.Name}();
{Outdent()}}}");

            // Optional Pre-processing
            this.codeOutput($@"{Indent()}ToDtoPreprocessing({AssignmentStrategy.DtoVariableName}, {AssignmentStrategy.ModelVariableName});");

            // Assign properties by convention if possible
            foreach (var assignment in map.DtoToModelMap.Values)
            {
                this.codeOutput($"{Indent()}{assignment.Source.CreateCodeLine(DtoOrModel.Model)}");
                this.codeOutput($"{Indent()}{assignment.Destination.CreateCodeLine(DtoOrModel.Dto, assignment.Source.SourceVariableName)}");
            }

            // Optional Post-processing
            this.codeOutput($@"{Indent()}ToDtoPostprocessing(ref {AssignmentStrategy.DtoVariableName}, {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}return {AssignmentStrategy.DtoVariableName};
{Outdent()}}} // End ToDto Method");

            // Partial Methods
            this.codeOutput($@"{Indent()}partial void ToModelPreprocessing({map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}partial void ToDtoPreprocessing({map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}partial void ModelFactory({map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, ref {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}partial void DtoFactory(ref {map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}partial void ToModelPostprocessing({map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, ref {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
            this.codeOutput($@"{Indent()}partial void ToDtoPostprocessing(ref {map.DtoType.Name} {AssignmentStrategy.DtoVariableName}, {map.ModelType.Name} {AssignmentStrategy.ModelVariableName});");
        }
    }
}