using System.Collections.Generic;
using System.Text;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public abstract class MappingGeneratorScenarios<TDto, TModel>
    {
        protected readonly List<string> DiagnosticMessages = new List<string>();
        protected readonly ITestOutputHelper Output;
        protected readonly MappingGenerator Subject;
        protected readonly string generatedCode;

        protected MappingGeneratorScenarios(ITestOutputHelper output)
        {
            this.Output = output;
            this.Subject = new MappingGenerator
            {
                DiagnosticLogging = l => this.DiagnosticMessages.Add(l)
            };

            this.generatedCode = Act();
        }

        protected string Act()
        {
            bool codeOutputed = false, errors = false;
            var code = new StringBuilder();
            this.Subject.Generate<TDto, TModel>(
                c =>
                {
                    codeOutputed = true;
                    code.AppendLine(c);
                    this.Output.WriteLine(c);
                },
                e =>
                {
                    errors = true;
                    this.Output.WriteLine(e);
                },
                w => this.Output.WriteLine(w));

            this.Output.WriteLine("===== Logging Messages: =====");
            this.DiagnosticMessages.ForEach(this.Output.WriteLine);

            Assert.True(codeOutputed);
            Assert.False(errors);

            return code.ToString();
        }

        protected IDtoMapper<TDto, TModel> CreateMapper()
        {
            var mapper = new DynamicCodeCompiler().CompileMapperCode<TDto, TModel>(this.generatedCode, this.Output);
            return mapper;
        }
    }
}