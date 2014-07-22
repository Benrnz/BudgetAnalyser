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
    public class DtoToIncomeMapper
    {
        private static bool isAutoMapperConfigured;

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
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.IncomeBucketCode, Result.Bucket.Code);
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

            Result = Mapper.Map<Income>(TestData);
        }
    }
}