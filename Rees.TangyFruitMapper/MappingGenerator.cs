using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Rees.TangyFruitMapper
{
    public class MappingGenerator
    {
        private Dictionary<PropertyInfo,PropertyInfo> mappedProperties = new Dictionary<PropertyInfo, PropertyInfo>();
        private Type dtoType;
        private Type modelType;
        private Action<string> codeOutput;
        private Action<string> errorOutput;
        private int indent = 0;

        public Action<string> DiagnosticLogging { get; set; }

        public void Generate<TDto, TModel>(
            [NotNull] Action<string> codeOutput,
            [NotNull] Action<string> errorOutput)
        {
            if (codeOutput == null) throw new ArgumentNullException(nameof(codeOutput));
            if (errorOutput == null) throw new ArgumentNullException(nameof(errorOutput));
            if (DiagnosticLogging == null) DiagnosticLogging = x => { };

            this.codeOutput = codeOutput;
            this.errorOutput = errorOutput;
            this.modelType = typeof (TModel);
            this.dtoType = typeof (TDto);
            DiagnosticLogging($"Starting to generate code for mapping {this.modelType.Name} to {this.dtoType.Name}...");

            Preconditions();
            WriteClassHeader();

            try
            {
                WriteMapToDtoHeader();
                foreach (var sourceProperty in this.modelType.GetProperties())
                {
                    DiagnosticLogging($"Looking for a match for source property '{sourceProperty.Name}'");
                    if (AttemptMapToProperty(sourceProperty)) continue;
                    // Attempt Field map 
                }
                WriteMapToDtoFooter();
            }
            catch (PropertyNotMatchedException ex)
            {
                this.errorOutput(ex.Message);
            }
            finally
            {
                WriteClassFooter();
            }
        }

        private void Preconditions()
        {
            var modelCtor = this.modelType.GetConstructor(new Type[] { });
            if (modelCtor == null)
            {
                throw new NoAccessibleDefaultConstructorException($"No constructor found on {this.modelType.Name}");
            }

            var dtoCtor = this.modelType.GetConstructor(new Type[] { });
            if (dtoCtor == null)
            {
                throw new NoAccessibleDefaultConstructorException($"No constructor found on {this.dtoType.Name}");
            }
        }

        private void WriteClassFooter()
        {
            this.codeOutput($@"{Outdent()}}} // End Class");
            this.codeOutput($@"{Outdent()}}} // End Namespace");
        }

        private void WriteMapToDtoFooter()
        {
            this.codeOutput($@"{Indent()}return dto;
{Outdent()}}} // End ToDto Method");
        }

        private void WriteMapToDtoHeader()
        {
            this.codeOutput($@"{Indent(true)}public {this.modelType.Name} ToModel({this.dtoType.Name} dto) {{ throw new NotImplementedException(); }}
{Indent()}public {this.dtoType.Name} ToDto({this.modelType.Name} model)
{Indent()}{{
{Indent(true)}var dto = new {this.dtoType.Name}();");
        }

        private void WriteClassHeader()
        {
            this.codeOutput($@"using System;
using System.CodeDom.Compiler;
using Rees.TangyFruitMapper;
using {this.modelType.Namespace};
using {this.dtoType.Namespace};
namespace GeneratedCode
{{
{Indent(true)}[GeneratedCode(""1.0"", ""Tangy Fruit Mapper"")]
{Indent()}public class Mapper_{this.dtoType.Name}_{this.modelType.Name} : IDtoMapper<{this.dtoType.Name}, {this.modelType.Name}>
{Indent()}{{");
        }

        private bool AttemptMapToProperty(PropertyInfo sourceModelProperty)
        {
            var destinationDtoProperty = FindDestinationDtoProperty(sourceModelProperty);
            if (destinationDtoProperty != null)
            {
                this.mappedProperties.Add(destinationDtoProperty, sourceModelProperty);
                this.codeOutput($"{Indent()}dto.{destinationDtoProperty.Name} = model.{sourceModelProperty.Name};");
                return true;
            }

            return false;
        }

        private PropertyInfo FindDestinationDtoProperty(PropertyInfo sourceProperty)
        {
            var destinationProperty = this.dtoType.GetProperty(sourceProperty.Name);
            if (destinationProperty != null)
            {
                DiagnosticLogging($"    Found match with same name on destination type.");
                if (destinationProperty.CanWrite)
                {
                    DiagnosticLogging($"    Property is writable, all is good.");
                    return destinationProperty;
                }
            }

            throw new PropertyNotMatchedException($"!!! ERROR - Destination property isn't writable.");
        }

        private string Indent(bool increment = false)
        {
            if (increment) this.indent++;
            return new string(' ', 4 * this.indent);
        }

        private string Outdent()
        {
            this.indent--;
            return new string(' ', 4 * this.indent);
        }
    }
}
