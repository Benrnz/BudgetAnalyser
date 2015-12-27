using System;
using System.Collections.Generic;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class MappingGeneratorTest
    {
        private readonly MappingGenerator subject;
        private readonly ITestOutputHelper output;
        private readonly List<string> diagnosticMessages = new List<string>();

        public MappingGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
            this.subject = new MappingGenerator
            {
                DiagnosticLogging = l => this.diagnosticMessages.Add(l)
            };
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenPublicSetters()
        {
            Act<DtoType1, ModelType1_AllWriteable>();
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenPrivateAutoSetters()
        {
            Act<DtoType2, ModelType2_PrivateSet>();
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenPrivateSetterWithBackingField()
        {
            throw new NotImplementedException();
            // TODO
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenDeepObjectGraph()
        {
            throw new NotImplementedException();
            // TODO
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenCollectionOfObjects()
        {
            throw new NotImplementedException();
            // TODO
        }

        private void Act<TDto, TModel>()
        {
            bool codeOutputed = false, errors = false;
            this.subject.Generate<TDto, TModel>(
                s =>
                {
                    codeOutputed = true;
                    this.output.WriteLine(s);
                },
                e =>
                {
                    errors = true;
                    this.output.WriteLine(e);
                },
                w => this.output.WriteLine(w));

            this.output.WriteLine("===== Logging Messages: =====");
            this.diagnosticMessages.ForEach(this.output.WriteLine);

            Assert.True(codeOutputed);
            Assert.False(errors);
        }
    }
}
