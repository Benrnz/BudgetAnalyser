using System.Collections.Generic;
using System.Linq;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario6_1Collection : MappingGeneratorScenarios<DtoType6, ModelType6_1Collection>
    {
        public Scenario6_1Collection(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(new ModelType6_1Collection(410, 3.1415M, new[] { "Horse", "Battery", "Stapler" }));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Horse", result.Names.First());
            Assert.Equal("Battery", result.Names.Skip(1).Take(1).Single());
            Assert.Equal("Stapler", result.Names.Last());
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType6
            {
                Age = 410,
                MyNumber = 3.1415M,
                Names = new List<string> { "Horse", "Battery", "Stapler" },
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Horse", result.Names.First());
            Assert.Equal("Battery", result.Names.Skip(1).Take(1).Single());
            Assert.Equal("Stapler", result.Names.Last());
        }
    }
}
