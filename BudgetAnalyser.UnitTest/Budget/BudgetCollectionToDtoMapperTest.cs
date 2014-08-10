using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToDtoMapperTest
    {
        private BudgetCollection TestData { get; set; }

        [TestMethod]
        public void BudgetsCountShouldBeMapped()
        {
            BudgetCollectionDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Count, result.Budgets.Count);
        }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            BudgetCollectionDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the BudgetCollectionDto. This is a trigger to update the mappers.")]
        public void NumberOfBudgetCollectionDtoPropertiesShouldBe3()
        {
            int dataProperties = typeof(BudgetCollectionDto).CountProperties();
            Assert.AreEqual(3, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateCollectionWith1And2();
        }

        private BudgetCollectionDto ArrangeAndAct()
        {
            var mapper = new BudgetCollectionToDtoMapper();
            return mapper.Map(TestData);
        }
    }
}