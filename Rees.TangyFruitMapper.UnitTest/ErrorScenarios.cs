using System.Collections.ObjectModel;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class ErrorScenarios
    {
        [Fact]
        public void ShouldThrow_GivenDtoWithNonpublicProperty()
        {
            Assert.Throws<PropertiesMustBePublicWriteableException>(() => Act<NonPublicPropertyDto, NonPublicPropertyModel>());
        }

        [Fact]
        public void ShouldThrow_GivenNoDefaultConstructorOnDto()
        {
            Assert.Throws<NoAccessibleDefaultConstructorException>(() => Act<NoDefaultConstructorDto, NonPublicPropertyModel>());
        }

        [Fact]
        public void ShouldThrow_GivenNoDefaultConstructorOnModel()
        {
            Assert.Throws<NoAccessibleDefaultConstructorException>(() => Act<NonPublicPropertyDto, NoDefaultConstructorDto>());
        }

        [Fact]
        public void ShouldThrow_GivenDtoCollectionsAreNotLists()
        {
            Assert.Throws<CollectionsMustBeListTException>(() => Act<CollectionsShouldBeLists, DtoType1>());
        }

        private static void Act<TDto, TModel>()
        {
            var subject = new MappingGenerator();
            subject.Generate<TDto, TModel>(s => { }, e => { }, w => { });
        }

        public class CollectionsShouldBeLists
        {
            public Collection<int> RandomInts { get; set; }
        }

        public class NoDefaultConstructorDto
        {
            private readonly string foo;

            internal NoDefaultConstructorDto(string foo)
            {
                this.foo = foo;
            }

            public string Name { get; set; }
        }

        public class NonPublicPropertyDto
        {
            public string Name { get; set; }

            protected double Spleen { get; set; }
        }

        public class NonPublicPropertyModel
        {
            public string Name { get; set; }

            protected double Spleen { get; set; }
        }
    }
}