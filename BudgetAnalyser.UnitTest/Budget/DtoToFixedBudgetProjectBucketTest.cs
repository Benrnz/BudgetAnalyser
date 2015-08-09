using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToFixedBudgetProjectBucketTest
    {
        private const string TestDataCode = "FOO";
        private BudgetBucket result;
        private BudgetBucketDto testData;

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(this.testData.Code, this.result.Code);
        }

        [TestMethod]
        public void ShouldMapDescription()
        {
            Assert.AreEqual(this.testData.Description, this.result.Description);
        }

        [TestMethod]
        public void ShouldMapFixedBudgetAmount()
        {
            Assert.AreEqual(((FixedBudgetBucketDto)this.testData).FixedBudgetAmount, ((FixedBudgetProjectBucket)this.result).FixedBudgetAmount);
        }

        [TestMethod]
        public void ShouldMapSubCode()
        {
            Assert.AreEqual(TestDataCode, ((FixedBudgetProjectBucket)this.result).SubCode);
        }

        [TestMethod]
        public void ShouldMapType()
        {
            Assert.IsInstanceOfType(this.result, typeof(FixedBudgetProjectBucket));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.testData = new FixedBudgetBucketDto
            {
                Code = FixedBudgetProjectBucket.CreateCode(TestDataCode),
                Description = "Foo bar tiddle-de-dum",
                FixedBudgetAmount = 2000
            };
            var subject = new DtoToBudgetBucketMapper();
            this.result = subject.Map(this.testData);
        }
    }
}