using System.Collections.Generic;
using System.Linq;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class NamespaceFinderTest
    {
        private readonly ITestOutputHelper output;

        public NamespaceFinderTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void DiscoverNestedNamespacesTest()
        {
            var subject = new NamespaceFinder(typeof(DtoType5), typeof(ModelType5_DeepObjectGraph));
            var result = subject.DiscoverNamespaces();
            result.ToList().ForEach(kvp => this.output.WriteLine($"{kvp.Key}    {kvp.Value}"));

            Assert.Equal(3, result.Count());
        }
    }
}
