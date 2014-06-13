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

        [TestMethod]
        public void NoDecreaseInTests()
        {
            var assembly = GetType().Assembly;
            int count = (from type in assembly.ExportedTypes let testClassAttrib = type.GetCustomAttribute<TestClassAttribute>() where testClassAttrib != null select type.GetMethods().Count(method => method.GetCustomAttribute<TestMethodAttribute>() != null)).Sum();
            Console.WriteLine(count);
            Assert.IsTrue(count >= 309);
        }
    }
}
