using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest
{
    [TestClass]
    public class MetaTest
    {
        private const int ExpectedMinimumTests = 867;

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
            int count = CountTests();
            Console.WriteLine(count);
            Assert.IsTrue(count >= ExpectedMinimumTests);
        }

        [TestMethod]
        public void UpdateNoDecreaseInTests()
        {
            int count = CountTests();

            Assert.IsFalse(count > ExpectedMinimumTests + 10, "Update the minimum expected number of tests to " + count);
        }

        private int CountTests()
        {
            Assembly assembly = GetType().Assembly;
            int count = (from type in assembly.ExportedTypes
                let testClassAttrib = type.GetCustomAttribute<TestClassAttribute>()
                where testClassAttrib != null
                select type.GetMethods().Count(method => method.GetCustomAttribute<TestMethodAttribute>() != null)).Sum();
            return count;
        }
    }
}