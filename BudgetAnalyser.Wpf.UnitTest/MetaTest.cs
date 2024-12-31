using System.Reflection;

namespace BudgetAnalyser.Wpf.UnitTest
{
    [TestClass]
    public class MetaTest
    {
        private const int MinimumTestCount = 9;

        [TestMethod]
        public void ListAllTests()
        {
            var assembly = GetType().Assembly;
            var count = 0;
            foreach (var type in assembly.ExportedTypes)
            {
                var testClassAttrib = type.GetCustomAttribute<TestClassAttribute>();
                if (testClassAttrib is not null)
                {
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<TestMethodAttribute>() is not null)
                        {
                            Console.WriteLine("{0} {1} - {2}", ++count, type.FullName, method.Name);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NoDecreaseInTests()
        {
            var assembly = GetType().Assembly;
            var count = (from type in assembly.ExportedTypes
                         let testClassAttrib = type.GetCustomAttribute<TestClassAttribute>()
                         where testClassAttrib is not null
                         select type.GetMethods().Count(method => method.GetCustomAttribute<TestMethodAttribute>() is not null)).Sum();
            Console.WriteLine(count);
            Assert.IsTrue(count >= MinimumTestCount);
        }
    }
}
