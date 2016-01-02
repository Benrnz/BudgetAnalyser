using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario4_PropertiesWithBackingFieldAndUnderscorePrefix : MappingGeneratorScenarios<DtoType4, ModelType4_UnderscoreBackingField>
    {
        public Scenario4_PropertiesWithBackingFieldAndUnderscorePrefix(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(new ModelType4_UnderscoreBackingField(410, 3.1415M, "Pie Constant"));

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
