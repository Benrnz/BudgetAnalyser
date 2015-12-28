using Rees.TangyFruitMapper.UnitTest.TestData;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario5_DeepObjectGraph : MappingGeneratorScenarios<DtoType5, ModelType5_DeepObjectGraph>
    {
        public Scenario5_DeepObjectGraph(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(new ModelType5_DeepObjectGraph(410, 3.1415M, new Name5("Cat", "Spew")));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Cat", result.Name.FirstName);
            Assert.Equal("Spew", result.Name.Surname);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType5
            {
                Age = 410,
                MyNumber = 3.1415M,
                Name = new NameDto5 { FirstName = "Cat", Surname = "Spew" },
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Cat", result.Name.FirstName);
            Assert.Equal("Spew", result.Name.Surname);
        }
    }
}
