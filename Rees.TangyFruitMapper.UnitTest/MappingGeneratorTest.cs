using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class MappingGeneratorTest
    {
        private MappingGenerator subject;
        private readonly ITestOutputHelper output;

        public MappingGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
            this.subject = new MappingGenerator
            {
                //DiagnosticLogging = l => this.output.WriteLine(l)
            };
        }

        [Fact]
        public void Generate_ShouldOutputCode()
        {
            bool codeOutputed = false, errors = false;
            this.subject.Generate<DtoType1, ModelType1>(
                s =>
                {
                    codeOutputed = true;
                    this.output.WriteLine(s);
                },
                e =>
                {
                    errors = true;
                    this.output.WriteLine(e);
                });

            Assert.True(codeOutputed);
            Assert.False(errors);
        }
    }
}
