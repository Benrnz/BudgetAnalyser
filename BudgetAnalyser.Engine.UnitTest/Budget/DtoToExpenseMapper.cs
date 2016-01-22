using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
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
            var subject = new Mapper_ExpenseDto_Expense(new BucketBucketRepoAlwaysFind());
            Result = subject.ToModel(TestData);
        }
    }
}