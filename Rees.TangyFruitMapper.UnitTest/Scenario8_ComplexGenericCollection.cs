using System.Collections.Generic;
using System.Linq;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class Scenario8_ComplexGenericCollection : MappingGeneratorScenarios<DtoType8, ModelType8_ComplexCollection>
    {
        public Scenario8_ComplexGenericCollection(ITestOutputHelper output) : base(output)
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
                new ModelType8_ComplexCollection(
                    410, 
                    3.1415M, 
                    new[]
                    {
                        new Name5("Stinky", "Poo"),
                        new Name5("Blue", "Fuzz"), 
                    }));

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            var first = result.Names.First();
            Assert.Equal("Stinky", first.FirstName);
            Assert.Equal("Poo", first.Surname);
            var last = result.Names.Last();
            Assert.Equal("Blue", last.FirstName);
            Assert.Equal("Fuzz", last.Surname);
        }

        [Fact]
        public void Generate_ShouldSuccessfullyMapToModel()
        {
            var mapper = CreateMapperFromGeneratedCode();
            var result = mapper.ToModel(new DtoType8
            {
                Age = 410,
                MyNumber = 3.1415M,
                Names = new List<NameDto5>
                {
                    new NameDto5 { FirstName = "Stinky", Surname = "Poo" },
                    new NameDto5 { FirstName = "Blue", Surname = "Fuzz" },
                },
            });

            Assert.Equal(410, result.Age);
            Assert.Equal(3.1415M, result.MyNumber);
            var first = result.Names.First();
            Assert.Equal("Stinky", first.FirstName);
            Assert.Equal("Poo", first.Surname);
            var last = result.Names.Last();
            Assert.Equal("Blue", last.FirstName);
            Assert.Equal("Fuzz", last.Surname);
        }
    }
}
