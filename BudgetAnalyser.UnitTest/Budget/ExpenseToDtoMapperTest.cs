using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class ExpenseToDtoMapperTest
    {
        private static bool isAutoMapperConfigured;

        private ExpenseDto Result { get; set; }

        private Expense TestData
        {
            get { return BudgetModelTestData.CreateTestData1().Expenses.First(e => e.Bucket.Code == TestDataConstants.CarMtcBucketCode); }
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.CarMtcBucketCode, Result.BudgetBucketCode);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(95M, Result.Amount);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            if (!isAutoMapperConfigured)
            {
                new AutoMapperConfiguration(new Mock<IBudgetBucketFactory>().Object, new Mock<IBudgetBucketRepository>().Object).Configure();
                isAutoMapperConfigured = true;
            }

            Result = Mapper.Map<ExpenseDto>(TestData);
        }
    }
}