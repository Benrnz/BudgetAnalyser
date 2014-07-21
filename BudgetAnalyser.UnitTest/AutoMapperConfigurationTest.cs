using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
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
            Subject.Configure();
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new AutoMapperConfiguration(new Mock<IBudgetBucketFactory>().Object, new Mock<IBudgetBucketRepository>().Object);
        }

        private AutoMapperConfiguration Subject { get; set; }
    }
}
