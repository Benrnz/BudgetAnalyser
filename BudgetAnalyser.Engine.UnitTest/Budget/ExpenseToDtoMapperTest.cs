using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class ExpenseToDtoMapperTest
    {
        private ExpenseDto Result { get; set; }
        private Expense TestData => BudgetModelTestData.CreateTestData1().Expenses.First(e => e.Bucket.Code == TestDataConstants.CarMtcBucketCode);

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(95M, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.CarMtcBucketCode, Result.BudgetBucketCode);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var subject = new Mapper_ExpenseDto_Expense(new BucketBucketRepoAlwaysFind());
            Result = subject.ToDto(TestData);
        }
    }
}