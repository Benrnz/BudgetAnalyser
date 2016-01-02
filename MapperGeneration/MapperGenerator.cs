using System.Text;
using System.Threading;
using System.Windows;
using Rees.TangyFruitMapper;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public abstract class MapperGenerator
    {
        private readonly MappingGenerator codeGenerator;
        private readonly StringBuilder diagnosticLogger;
        private readonly StringBuilder generatedCode;
        private readonly ITestOutputHelper output;

        protected MapperGenerator(ITestOutputHelper output)
        {
            this.output = output;
            this.generatedCode = new StringBuilder();
            this.diagnosticLogger = new StringBuilder();
            this.codeGenerator = new MappingGenerator
            {
                DiagnosticLogging = l => this.diagnosticLogger.AppendLine(l),
                EmitWithInternalAccessors = true,
            };
        }

        protected string GeneratedCode => this.generatedCode.ToString();

        protected void Act<TDto, TModel>(string @namespace)
        {
            try
            {
                this.codeGenerator.Namespace = @namespace;
                this.codeGenerator.Generate<TDto, TModel>(codeLine =>
                {
                    this.generatedCode.AppendLine(codeLine);
                    this.output.WriteLine(codeLine);
                });
            }
            finally
            {
                this.output.WriteLine(string.Empty);
                this.output.WriteLine("===========================================================================");
                this.output.WriteLine(this.diagnosticLogger.ToString());
            }

            var staThread = new Thread(() => Clipboard.SetText(GeneratedCode));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }
    }
}