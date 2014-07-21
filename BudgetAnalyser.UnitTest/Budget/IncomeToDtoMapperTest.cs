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
    public class IncomeToDtoMapperTest
    {
        private static bool isAutoMapperConfigured;

        private IncomeDto Result { get; set; }

        private Income TestData
        {
            get { return BudgetModelTestData.CreateTestData1().Incomes.First(e => e.Bucket.Code == TestDataConstants.IncomeBucketCode); }
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.IncomeBucketCode, Result.BudgetBucketCode);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(1500M, Result.Amount);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            if (!isAutoMapperConfigured)
            {
                new AutoMapperConfiguration(new Mock<IBudgetBucketFactory>().Object, new Mock<IBudgetBucketRepository>().Object).Configure();
                isAutoMapperConfigured = true;
            }

            Result = Mapper.Map<IncomeDto>(TestData);
        }
    }
}