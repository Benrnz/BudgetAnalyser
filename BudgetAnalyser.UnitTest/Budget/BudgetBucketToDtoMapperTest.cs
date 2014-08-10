using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketToDtoMapperTest
    {
        private BudgetBucketDto Result { get; set; }

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

        [TestMethod]
        public void ShouldMapType()
        {
            Assert.IsInstanceOfType(Result, typeof(BudgetBucketDto));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            SpentMonthlyExpenseBucket testData = StatementModelTestData.CarMtcBucket;
            Result = Mapper.Map<BudgetBucketDto>(testData);
        }
    }
}