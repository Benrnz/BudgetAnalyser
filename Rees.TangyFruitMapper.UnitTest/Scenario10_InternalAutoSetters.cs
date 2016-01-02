using System;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario10_PropertiesWithInternalAutoSettersTest : MappingGeneratorScenarios<DtoType10, ModelType10_InternalSet>
    {
        private readonly Guid myGuid = new Guid("{B7B6A136-6813-4B76-A450-D497DAA2E926}");

        public Scenario10_PropertiesWithInternalAutoSettersTest(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(new ModelType10_InternalSet(410, this.myGuid, "Pie Constant"));

            Assert.Equal(410, result.Age);
            Assert.Equal(this.myGuid, result.FiddleStick);
            Assert.Equal("Pie Constant", result.Name);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType10
            {
                Age = 410,
                FiddleStick = this.myGuid,
                Name = "Pie Constant"
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(this.myGuid, result.FiddleStick);
            Assert.Equal("Pie Constant", result.Name);
        }
    }
}