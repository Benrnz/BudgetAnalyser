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
        private int indent = 0;
        private NamespaceFinder namespaceFinder;

        public Action<string> DiagnosticLogging { get; set; }

        public void Generate<TDto, TModel>(
            [NotNull] Action<string> codeOutput,
            [NotNull] Action<string> errorOutput, 
            [NotNull] Action<string> warnOutput)
        {
            if (codeOutput == null) throw new ArgumentNullException(nameof(codeOutput));
            if (errorOutput == null) throw new ArgumentNullException(nameof(errorOutput));
            if (warnOutput == null) throw new ArgumentNullException(nameof(warnOutput));
            if (DiagnosticLogging == null) DiagnosticLogging = x => { };

            this.codeOutput = codeOutput;
            this.errorOutput = errorOutput;
            this.modelType = typeof (TModel);
            this.dtoType = typeof (TDto);
            this.namespaceFinder = new NamespaceFinder(this.dtoType, this.modelType);
            DiagnosticLogging($"Starting to generate code for mapping {this.modelType.Name} to {this.dtoType.Name}...");

            MapByProperties map = null;
            try
            {
                map = new MapByProperties(DiagnosticLogging, this.dtoType, this.modelType);
                map.CreateMap();
            }
            catch (PropertyNotMatchedException ex)
            {
                this.errorOutput(ex.Message);
                return;
            }

            WriteClassHeader();
            WriteMethods(map);
            WriteClassFooter();

            map.Warnings.ToList().ForEach(warnOutput);
        }

        private void WriteClassFooter()
        {
            this.codeOutput($@"{Outdent()}}} // End Class");
            this.codeOutput($@"{Outdent()}}} // End Namespace
");
        }

        private void WriteMethods(MapByProperties map)
        {
            this.codeOutput($@"{Indent()}public {this.modelType.Name} ToModel({this.dtoType.Name} dto)
{Indent()}{{
{Indent(true)}var model = new {this.modelType.Name}();");
            foreach (var assignment in map.ModelToDtoMap.Values)
            {
                this.codeOutput($"{Indent()}{assignment.CreateCodeLine()}");
            }
            this.codeOutput($@"{Indent()}return model;
{Outdent()}}} // End ToModel Method");


            this.codeOutput($@"
{Indent()}public {this.dtoType.Name} ToDto({this.modelType.Name} model)
{Indent()}{{
{Indent(true)}var dto = new {this.dtoType.Name}();");
            foreach (var assignment in map.DtoToModelMap.Values)
            {
                this.codeOutput($"{Indent()}{assignment.CreateCodeLine()}");
            }
            this.codeOutput($@"{Indent()}return dto;
{Outdent()}}} // End ToDto Method");
        }

        private void WriteClassHeader()
        {
            this.codeOutput($@"using System;
using System.CodeDom.Compiler;
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
