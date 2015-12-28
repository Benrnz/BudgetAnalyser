using System;
using System.Linq;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    public class MappingGenerator
    {
        private Type dtoType;
        private Type modelType;
        private Action<string> codeOutput;
        private Action<string> errorOutput;
        private int indent;
        private NamespaceFinder namespaceFinder;

        public Action<string> DiagnosticLogging { get; set; }

        public void Generate<TDto, TModel>(
            [NotNull] Action<string> codeOutputDelegate,
            [NotNull] Action<string> errorOutputDelegate, 
            [NotNull] Action<string> warnOutputDelegate)
        {
            if (codeOutputDelegate == null) throw new ArgumentNullException(nameof(codeOutputDelegate));
            if (errorOutputDelegate == null) throw new ArgumentNullException(nameof(errorOutputDelegate));
            if (warnOutputDelegate == null) throw new ArgumentNullException(nameof(warnOutputDelegate));
            if (DiagnosticLogging == null) DiagnosticLogging = x => { };

            this.codeOutput = codeOutputDelegate;
            this.errorOutput = errorOutputDelegate;
            this.modelType = typeof (TModel);
            this.dtoType = typeof (TDto);
            this.namespaceFinder = new NamespaceFinder(this.dtoType, this.modelType);
            DiagnosticLogging($"Starting to generate code for mapping {this.modelType.Name} to {this.dtoType.Name}...");

            MapByProperties mapper;
            MapResult mapResult;
            try
            {
                mapper = new MapByProperties(DiagnosticLogging, this.dtoType, this.modelType);
                mapResult = mapper.CreateMap();
            }
            catch (PropertyNotMatchedException ex)
            {
                this.errorOutput(ex.Message);
                return;
            }

            WriteClassHeader();
            WriteMethods(mapResult);
            WriteClassFooter();

            mapper.Warnings.ToList().ForEach(warnOutputDelegate);
        }

        private void WriteClassFooter()
        {
            this.codeOutput($@"{Outdent()}}} // End Class");
            this.codeOutput($@"{Outdent()}}} // End Namespace
");
        }

        private void WriteMethods(MapResult map)
        {
            this.codeOutput($@"{Indent()}public {this.modelType.Name} ToModel({this.dtoType.Name} {AssignmentStrategy.DtoVariableName})
{Indent()}{{
{Indent(true)}var {AssignmentStrategy.ModelVariableName} = new {this.modelType.Name}();
{Indent()}var {AssignmentStrategy.ModelTypeVariableName} = {AssignmentStrategy.ModelVariableName}.GetType();");
            foreach (var assignment in map.ModelToDtoMap.Values)
            {
                // model.Property = dto.Property;
                this.codeOutput($"{Indent()}{assignment.Source.CreateCodeLine(DtoOrModel.Dto)}");
                this.codeOutput($"{Indent()}{assignment.Destination.CreateCodeLine(DtoOrModel.Model, assignment.Source.SourceVariableName)}");
            }
            this.codeOutput($@"{Indent()}return {AssignmentStrategy.ModelVariableName};
{Outdent()}}} // End ToModel Method");


            this.codeOutput($@"
{Indent()}public {this.dtoType.Name} ToDto({this.modelType.Name} {AssignmentStrategy.ModelVariableName})
{Indent()}{{
{Indent(true)}var {AssignmentStrategy.DtoVariableName} = new {this.dtoType.Name}();");
            foreach (var assignment in map.DtoToModelMap.Values)
            {
                this.codeOutput($"{Indent()}{assignment.Source.CreateCodeLine(DtoOrModel.Model)}");
                this.codeOutput($"{Indent()}{assignment.Destination.CreateCodeLine(DtoOrModel.Dto, assignment.Source.SourceVariableName)}");
            }
            this.codeOutput($@"{Indent()}return {AssignmentStrategy.DtoVariableName};
{Outdent()}}} // End ToDto Method");
        }

        private void WriteClassHeader()
        {
            this.codeOutput($@"using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Rees.TangyFruitMapper;");
            foreach (var ns in this.namespaceFinder.DiscoverNamespaces())
            {
                this.codeOutput($@"using {ns};");
            }
            this.codeOutput($@"
namespace GeneratedCode
{{
{Indent(true)}[GeneratedCode(""1.0"", ""Tangy Fruit Mapper"")]
{Indent()}public class Mapper_{this.dtoType.Name}_{this.modelType.Name} : IDtoMapper<{this.dtoType.Name}, {this.modelType.Name}>
{Indent()}{{
{Indent(true)}");
        }

        private string Indent(bool increment = false)
        {
            if (increment) this.indent++;
            return new string(' ', 4 * this.indent);
        }

        private string Outdent()
        {
            this.indent--;
            if (this.indent < 0) this.indent = 0;
            return new string(' ', 4 * this.indent);
        }
    }
}
