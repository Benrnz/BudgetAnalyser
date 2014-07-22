using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class AutoMapperConfigurationTest
    {
        [TestMethod]
        public void IsConfigurationValid()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = AutoMapperConfiguration();
        }

        private AutoMapperConfiguration Subject { get; set; }

        public static AutoMapperConfiguration AutoMapperConfiguration()
        {
            return new AutoMapperConfiguration(new BudgetBucketFactory(), new BucketBucketRepoAlwaysFind()).Configure();
        }
    }
}
