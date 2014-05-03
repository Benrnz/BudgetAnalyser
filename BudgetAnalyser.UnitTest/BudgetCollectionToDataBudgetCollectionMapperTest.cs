using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BudgetCollectionToDataBudgetCollectionMapperTest
    {
        private BudgetCollection TestData { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateCollectionWith1And2();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataBudgetModel. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe5()
        {
            var dataProperties = CountProperties(typeof (DataBudgetCollection));
            Assert.AreEqual(2, dataProperties);
        }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
        }

        [TestMethod]
        public void BudgetsCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Count, result.Budgets.Count);
        }

        private DataBudgetCollection ArrangeAndAct()
        {
            var mapper = new BudgetCollectionToDataBudgetCollectionMapper(new BudgetModelToDataBudgetModelMapper());
            return mapper.Map(TestData);
        }

        private int CountProperties(Type type)
        {
            var properties = type.GetProperties();
            properties.ToList().ForEach(p => Console.WriteLine(p.Name));
            return properties.Length;
        }
    }
}
