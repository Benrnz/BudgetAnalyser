using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario3_PropertiesWithBackingFields : MappingGeneratorScenarios<DtoType3, ModelType3_BackingField>
    {
        public Scenario3_PropertiesWithBackingFields(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(new ModelType3_BackingField(410, 3.1415M, "Pie Constant"));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Pie Constant", result.Name);
        }

        [Fact]
        public void Generate_ShouldNOTMapToModel()
        {
            // This is not supported.  Properties must be writable at least with private setters.
            Assert.True(this.GeneratedCode.Contains("// TODO No properties found to map"));
        }
    }
}
