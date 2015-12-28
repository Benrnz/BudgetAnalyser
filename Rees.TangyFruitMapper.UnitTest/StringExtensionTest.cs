using Xunit;
using Xunit.Abstractions;

namespace Rees.TangyFruitMapper.UnitTest
{
    public class StringExtensionTest
    {
        private readonly ITestOutputHelper output;

        public StringExtensionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("MyNumber", "myNumber")]
        [InlineData("A", "A")]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("myNumber", "myNumber")]
        public void ConvertPascalCaseToCamelCase(string input, string expected)
        {
            var result = input.ConvertPascalCaseToCamelCase();
            this.output.WriteLine($"{input} => {result}");
            Assert.Equal(expected, result);
        }
    }
}
