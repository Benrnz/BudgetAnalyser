using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketToDtoMapperTest
    {
        private static bool configurationDone;

        [TestMethod]
        public void ShouldMapType()
        {
            Assert.IsInstanceOfType(Result, typeof(BudgetBucketDto));
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.CarMtcBucketCode, Result.Code);
        }

        [TestMethod]
        public void ShouldMapDescription()
        {
            Assert.AreEqual("Car Maintenance", Result.Description);
        }
        
        [TestInitialize]
        public void TestInitialise()
        {
            if (!configurationDone)
            {
                new AutoMapperConfiguration(new BudgetBucketFactory()).Configure();
                configurationDone = true;
            }

            var testData = StatementModelTestData.CarMtcBucket;
            Result = Mapper.Map<BudgetBucketDto>(testData);
        }

        private BudgetBucketDto Result { get; set; }
    }
}
