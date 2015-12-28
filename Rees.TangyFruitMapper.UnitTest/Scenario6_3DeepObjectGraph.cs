using Rees.TangyFruitMapper.UnitTest.TestData;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario6_3DeepObjectGraph : MappingGeneratorScenarios<DtoType6, ModelType6_3DeepObjectGraph>
    {
        public Scenario6_3DeepObjectGraph(ITestOutputHelper output) : base(output)
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
            var result = mapper.ToDto(
                new ModelType6_3DeepObjectGraph(
                    410, 
                    3.1415M, 
                    new Name6(
                        "Cat", 
                        "Spew",
                        new Address6
                        {
                            City = "Auckland",
                            Line1 = "High Street",
                            PostalCode = 1001,
                            StreetNumber = "3a",
                        })));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Cat", result.Name.FirstName);
            Assert.Equal("Spew", result.Name.Surname);
            Assert.Equal("Auckland", result.Name.StreetAddress.City);
            Assert.Equal("High Street", result.Name.StreetAddress.Line1);
            Assert.Equal("3a", result.Name.StreetAddress.StreetNumber);
            Assert.Equal(1001, result.Name.StreetAddress.PostalCode);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType6
            {
                Age = 410,
                MyNumber = 3.1415M,
                Name = new NameDto6
                {
                    FirstName = "Cat",
                    Surname = "Spew",
                    StreetAddress = new AddressDto6
                    {
                        City = "Auckland",
                        Line1 = "High Street",
                        StreetNumber = "3a",
                        PostalCode = 1001,
                    }
                },
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            Assert.Equal("Cat", result.Name.FirstName);
            Assert.Equal("Spew", result.Name.Surname);
        }
    }
}
