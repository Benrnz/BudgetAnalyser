using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
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
            Result = Mapper.Map<ExpenseDto>(TestData);
        }
    }
}