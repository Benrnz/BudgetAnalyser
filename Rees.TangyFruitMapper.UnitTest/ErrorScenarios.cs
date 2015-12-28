using System.Collections.Generic;
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
        public void ShouldThrow_GivenDtoCollection()
        {
            Assert.Throws<CollectionsMustBeListTException>(() => Act<CollectionsNotSupported, DtoType1>());
        }

        [Fact]
        public void ShouldThrow_GivenDtoArray()
        {
            Assert.Throws<CollectionsMustBeListTException>(() => Act<ArraysNotSupported, DtoType1>());
        }

        [Fact]
        public void ShouldThrow_GivenDtoIList()
        {
            Assert.Throws<CollectionsMustBeListTException>(() => Act<IListNotSupported, DtoType1>());
        }

        [Fact]
        public void ShouldThrow_GivenDtoIEnumerable()
        {
            Assert.Throws<CollectionsMustBeListTException>(() => Act<IEnumerableNotSupported, DtoType1>());
        }

        private static void Act<TDto, TModel>()
        {
            var subject = new MappingGenerator();
            subject.Generate<TDto, TModel>(s => { }, e => { }, w => { });
        }

        public class CollectionsNotSupported
        {
            public Collection<int> RandomInts { get; set; }
        }

        public class IListNotSupported
        {
            public IList<int> RandomInts { get; set; }
        }

        public class IEnumerableNotSupported
        {
            public IEnumerable<int> RandomInts { get; set; }
        }

        public class ArraysNotSupported
        {
            public int[] RandomInts { get; set; }
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