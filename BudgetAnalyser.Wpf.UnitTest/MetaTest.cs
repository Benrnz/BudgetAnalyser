using System.Reflection;

namespace BudgetAnalyser.Wpf.UnitTest
{
    [TestClass]
    public class MetaTest
    {
        private const int MinimumTestCount = 7;

        [TestMethod]
        public void ListAllTests()
        {
            Assembly assembly = GetType().Assembly;
            var count = 0;
            foreach (Type type in assembly.ExportedTypes)
            {
                var testClassAttrib = type.GetCustomAttribute<TestClassAttribute>();
                if (testClassAttrib != null)
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<TestMethodAttribute>() != null)
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
            Assembly assembly = GetType().Assembly;
            int count = (from type in assembly.ExportedTypes
                let testClassAttrib = type.GetCustomAttribute<TestClassAttribute>()
                where testClassAttrib != null
                select type.GetMethods().Count(method => method.GetCustomAttribute<TestMethodAttribute>() != null)).Sum();
            Console.WriteLine(count);
            Assert.IsTrue(count >= MinimumTestCount);
        }
    }
}