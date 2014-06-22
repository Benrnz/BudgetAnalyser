using System;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToDataBudgetCollectionMapperTest
    {
        private BudgetCollection TestData { get; set; }

        [TestMethod]
        public void BudgetsCountShouldBeMapped()
        {
            DataBudgetCollection result = ArrangeAndAct();
            Assert.AreEqual(TestData.Count, result.Budgets.Count);
        }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            DataBudgetCollection result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataBudgetModel. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe5()
        {
            int dataProperties = typeof(DataBudgetCollection).CountProperties();
            Assert.AreEqual(2, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateCollectionWith1And2();
        }

        private DataBudgetCollection ArrangeAndAct()
        {
            var mapper = new BudgetCollectionToDataBudgetCollectionMapper(new BudgetModelToDataBudgetModelMapper());
            return mapper.Map(TestData);
        }
    }
}