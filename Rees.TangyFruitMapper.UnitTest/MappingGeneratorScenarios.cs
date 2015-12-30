using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public abstract class MappingGeneratorScenarios<TDto, TModel>
    {
        protected readonly List<string> DiagnosticMessages = new List<string>();
        protected readonly ITestOutputHelper Output;
        protected readonly MappingGenerator Subject;
        protected readonly string GeneratedCode;

        protected MappingGeneratorScenarios(ITestOutputHelper output)
        {
            this.Output = output;
            this.Subject = new MappingGenerator
            {
                DiagnosticLogging = l => this.DiagnosticMessages.Add(l)
            };

            this.GeneratedCode = Act();
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
                });

            this.Output.WriteLine("===== Logging Messages: =====");
            this.DiagnosticMessages.ForEach(this.Output.WriteLine);

            Assert.True(codeOutputed);
            Assert.False(errors);

            return code.ToString();
        }

        protected IDtoMapper<TDto, TModel> CreateMapperFromGeneratedCode()
        {
            var mapper = new DynamicCodeCompiler().CompileMapperCode<TDto, TModel>(this.GeneratedCode, this.Output);
            return mapper;
        }
    }
}