using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToBudgetCollectionAutoMapperTest
    {
        private BudgetCollection Result { get; set; }

        private BudgetCollectionDto TestData { get; set; }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.FileName, Result.FileName);
        }

        [TestMethod]
        public void ShouldMapSameNumberOfBudgets()
        {
            Assert.AreEqual(TestData.Budgets.Count, Result.Count);
        }

        [TestMethod]
        public void ShouldMapBudgetsCorrectly()
        {
            Assert.AreEqual(TestData.Budgets.Sum(b => b.Expenses.Sum(e => e.Amount)), Result.Sum(b => b.Expenses.Sum(e => e.Amount)));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            // TODO Try AssemblyInitialise
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            // TODO roll this out to all other tests
            TestData = EmbeddedResourceHelper.Extract<BudgetCollectionDto>("BudgetAnalyser.UnitTest.TestData.BudgetCollectionTestData.xml");
            Result = Mapper.Map<BudgetCollection>(TestData);
        }
    }
}