using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToIncomeMapper
    {
        private Income Result { get; set; }

        private IncomeDto TestData
        {
            get
            {
                return new IncomeDto
                {
                    Amount = 1444.22M,
                    BudgetBucketCode = TestDataConstants.IncomeBucketCode,
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
            Assert.AreEqual(TestDataConstants.IncomeBucketCode, Result.Bucket.Code);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            Result = Mapper.Map<Income>(TestData);
        }
    }
}