using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class MetaTest
    {
        [TestMethod]
        public void ListAllTests()
        {
            var assembly = GetType().Assembly;
            int count = 0;
            foreach (var type in assembly.ExportedTypes)
            {
                var testClassAttrib = type.GetCustomAttribute<TestClassAttribute>();
                if (testClassAttrib != null)
                {
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<TestMethodAttribute>() != null)
                        {
                            Console.WriteLine("{0} {1} - {2}", ++count, type.FullName, method.Name);
                        }
                    }
                }
            }
        }

        private const int ExpectedMinimumTests = 680;

        [TestMethod]
        public void NoDecreaseInTests()
        {
            var count = CountTests();
            Console.WriteLine(count);
            Assert.IsTrue(count >= ExpectedMinimumTests);
        }

        [TestMethod]
        public void UpdateNoDecreaseInTests()
        {
            var count = CountTests();
            
            Assert.IsFalse(count > ExpectedMinimumTests + 10, "Update the minimum expected number of tests to " + count);
        }

        private int CountTests()
        {
            var assembly = GetType().Assembly;
            int count = (from type in assembly.ExportedTypes
                let testClassAttrib = type.GetCustomAttribute<TestClassAttribute>()
                where testClassAttrib != null
                select type.GetMethods().Count(method => method.GetCustomAttribute<TestMethodAttribute>() != null)).Sum();
            return count;
        }
    }
}
