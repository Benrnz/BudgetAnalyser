using System;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario9_InternalConstructor : MappingGeneratorScenarios<DtoType9, ModelType9_InternalConstructor>
    {
        public Scenario9_InternalConstructor(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Generate_ShouldOutputCode()
        {
            Assert.NotEmpty(this.GeneratedCode);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToDto()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToDto(new ModelType9_InternalConstructor("Cats Bum", 9, new DateTime(2006, 12, 2)));

            Assert.Equal(9, result.Age);
            Assert.Equal(new DateTime(2006, 12, 2), result.Dob);
            Assert.Equal("Cats Bum", result.Name);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType9
            {
                Age = 9,
                Dob = new DateTime(2006, 12, 2),
                Name = "Pie Constant"
            });

            Assert.Equal(9, result.Age);
            Assert.Equal(new DateTime(2006, 12, 2), result.Dob);
            Assert.Equal("Pie Constant", result.Name);
        }
    }
}