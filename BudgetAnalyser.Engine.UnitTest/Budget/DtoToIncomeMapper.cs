using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class DtoToIncomeMapper
    {
        private Income Result { get; set; }

        private IncomeDto TestData => new IncomeDto
        {
            Amount = 1444.22M,
            BudgetBucketCode = TestDataConstants.IncomeBucketCode
        };

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(1444.22M, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.IncomeBucketCode, Result.Bucket.Code);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var subject = new MapperIncomeDto2Income(new BucketBucketRepoAlwaysFind());
            Result = subject.ToModel(TestData);
        }
    }
}
