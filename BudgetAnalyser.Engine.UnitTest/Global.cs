using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class Global
    {
        public static AutoMapperConfiguration AutoMapperConfiguration { get; private set; }

        [AssemblyInitialize]
        public static void AssemblyInitialise(TestContext context)
        {
            AutoMapperConfiguration = AutoMapperConfigurationTest.AutoMapperConfiguration();
        }
    }
}