using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Rees.TangyFruitMapper.Validation;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class ErrorScenarios
    {
        private readonly ITestOutputHelper output;

        public ErrorScenarios(ITestOutputHelper output)
        {
            this.output = output;
        }

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

        // ***BR 29/12/2015 - Commented out to allow collections for now until theres a reason not to.
        //[Fact]
        //public void ShouldThrow_GivenDtoCollection()
        //{
        //    Assert.Throws<CollectionsMustBeListTException>(() => Act<CollectionsNotSupported, DtoType1>());
        //}

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

        [Fact]
        public void ShouldNotMapNonWriteableProperties()
        {
            var code = new StringBuilder();
            var codeOutput = new Action<string>(s => code.AppendLine(s));
            Act<DtoType1, IgnoreNonwriteableProperties>(codeOutput);
            this.output.WriteLine(code.ToString());
            Assert.False(code.ToString().Contains("WeirdCalculation"));
        }

        private static void Act<TDto, TModel>(Action<string> codeOutput = null)
        {
            var subject = new MappingGenerator();
            var codeWritter = codeOutput ?? (s => { });
            subject.Generate<TDto, TModel>(codeWritter);
        }

        public class IgnoreNonwriteableProperties
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public decimal MyNumber { get; set; }

            public decimal WeirdCalculation => Age * MyNumber;
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