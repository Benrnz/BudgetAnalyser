using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToExpenseMapper
    {
        private static bool isAutoMapperConfigured;

        private Expense Result { get; set; }

        private ExpenseDto TestData
        {
            get
            {
                return new ExpenseDto
                {
                    Amount = 1444.22M,
                    BudgetBucketCode = TestDataConstants.DoctorBucketCode,
                };
            }
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.DoctorBucketCode, Result.Bucket.Code);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(1444.22M, Result.Amount);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            if (!isAutoMapperConfigured)
            {
                AutoMapperConfigurationTest.AutoMapperConfiguration();
                isAutoMapperConfigured = true;
            }

            Result = Mapper.Map<Expense>(TestData);
        }
    }
}