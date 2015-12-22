using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToExpenseMapper
    {
        private Expense Result { get; set; }

        private ExpenseDto TestData
        {
            get
            {
                return new ExpenseDto
                {
                    Amount = 1444.22M,
                    BudgetBucketCode = TestDataConstants.DoctorBucketCode
                };
            }
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(1444.22M, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.DoctorBucketCode, Result.Bucket.Code);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<Expense>(TestData);
        }
    }
}