using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketToDtoMapperTest
    {
        private BudgetBucketDto Result { get; set; }

        [TestMethod]
        public void ShouldMapBucketType()
        {
            Assert.AreEqual(BucketDtoType.SavedUpForExpense, Result.Type);
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

        [TestMethod]
        public void ShouldMapType()
        {
            Assert.IsInstanceOfType(Result, typeof(BudgetBucketDto));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var testData = StatementModelTestData.CarMtcBucket;
            var subject = new Mapper_BudgetBucketDto_BudgetBucket(new BudgetBucketFactory());
            Result = subject.ToDto(testData);
        }
    }
}