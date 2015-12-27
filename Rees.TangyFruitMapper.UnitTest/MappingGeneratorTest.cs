using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class MappingGeneratorTest
    {
        private readonly List<string> diagnosticMessages = new List<string>();
        private readonly ITestOutputHelper output;
        private readonly MappingGenerator subject;

        public MappingGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
            this.subject = new MappingGenerator
            {
                DiagnosticLogging = l => this.diagnosticMessages.Add(l)
            };
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel_GivenPublicSetters()
        {
            var code = Act<DtoType1, ModelType1_AllWriteable>();
            var mapper = new DynamicCodeCompiler().CompileMapperCode<DtoType1, ModelType1_AllWriteable>(code, this.output);
            var result = mapper.ToModel(new DtoType1
            {
                Age = 410,
                MyNumber = 3.1415M,
                Name = "Pie Constant",
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Pie Constant", result.Name);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToDto_GivenPublicSetters()
        {
            var code = Act<DtoType1, ModelType1_AllWriteable>();
            var mapper = new DynamicCodeCompiler().CompileMapperCode<DtoType1, ModelType1_AllWriteable>(code, this.output);
            var result = mapper.ToDto(new ModelType1_AllWriteable
            {
                Age = 410,
                MyNumber = 3.1415M,
                Name = "Pie Constant",
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Pie Constant", result.Name);
        }

        [Fact]
        public void Generate_ShouldOutputCode_GivenCollectionOfObjects()
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
        public void Generate_ShouldOutputCode_GivenPublicSetters()
        {
            Act<DtoType1, ModelType1_AllWriteable>();
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel_GivenPrivateAutoSetters()
        {
            var code = Act<DtoType2, ModelType2_PrivateSet>();
            var mapper = new DynamicCodeCompiler().CompileMapperCode<DtoType2, ModelType2_PrivateSet>(code, this.output);
            var result = mapper.ToModel(new DtoType2
            {
                Age = 410,
                MyNumber = 3.1415M,
                Name = "Pie Constant",
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Pie Constant", result.Name);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToDto_GivenPrivateAutoSetters()
        {
            var code = Act<DtoType2, ModelType2_PrivateSet>();
            var mapper = new DynamicCodeCompiler().CompileMapperCode<DtoType2, ModelType2_PrivateSet>(code, this.output);
            var result = mapper.ToDto(new ModelType2_PrivateSet(410, 3.1415M, "Pie Constant"));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Pie Constant", result.Name);
        }

        private string Act<TDto, TModel>()
        {
            bool codeOutputed = false, errors = false;
            var code = new StringBuilder();
            this.subject.Generate<TDto, TModel>(
                c =>
                {
                    codeOutputed = true;
                    code.AppendLine(c);
                    this.output.WriteLine(c);
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

            return code.ToString();
        }
    }
}