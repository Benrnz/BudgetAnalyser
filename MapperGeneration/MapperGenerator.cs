using System.Text;
using Rees.TangyFruitMapper;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public abstract class MapperGenerator
    {
        private readonly ITestOutputHelper output;
        private readonly MappingGenerator codeGenerator;
        private readonly StringBuilder diagnosticLogger;

        protected MapperGenerator(ITestOutputHelper output)
        {
            this.output = output;
            this.diagnosticLogger = new StringBuilder();
            this.codeGenerator = new MappingGenerator
            {
                DiagnosticLogging = l => this.diagnosticLogger.AppendLine(l),
            };
        }

        protected void Act<TDto, TModel>()
        {
            try
            {
                this.codeGenerator.Generate<TDto, TModel>(this.output.WriteLine);
            }
            finally
            {
                this.output.WriteLine(string.Empty);
                this.output.WriteLine("===========================================================================");
                this.output.WriteLine(this.diagnosticLogger.ToString());
            }
        }
    }
}