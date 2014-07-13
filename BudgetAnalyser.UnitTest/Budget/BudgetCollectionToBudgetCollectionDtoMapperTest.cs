using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToBudgetCollectionDtoMapperTest
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
        [Description("A test designed to break when new propperties are added to the BudgetModelDto. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe2()
        {
            int dataProperties = typeof(BudgetCollectionDto).CountProperties();
            Assert.AreEqual(2, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateCollectionWith1And2();
        }

        private BudgetCollectionDto ArrangeAndAct()
        {
            var mapper = new BudgetCollectionToBudgetCollectionDtoMapper(new BudgetModelToBudgetModelDtoMapper(), new BucketBucketRepoAlwaysFind(), new BudgetBucketToBudgetBucketDtoMapper(new BudgetBucketFactory()));
            return mapper.Map(TestData);
        }
    }
}