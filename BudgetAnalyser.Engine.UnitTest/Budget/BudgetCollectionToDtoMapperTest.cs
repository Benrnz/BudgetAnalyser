using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToDtoMapperTest
    {
        private BudgetCollection TestData { get; set; }

        [TestMethod]
        public void BudgetsCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Count, result.Budgets.Count);
        }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.StorageKey, result.StorageKey);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the BudgetCollectionDto. This is a trigger to update the mappers.")]
        public void NumberOfBudgetCollectionDtoPropertiesShouldBe3()
        {
            var dataProperties = typeof(BudgetCollectionDto).CountProperties();
            Assert.AreEqual(3, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateCollectionWith1And2();
        }

        private BudgetCollectionDto ArrangeAndAct()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var mapper = new MapperBudgetCollectionDtoBudgetCollection(
                bucketRepo,
                new MapperBudgetBucketDtoBudgetBucket(new BudgetBucketFactory()),
                new MapperBudgetModelDtoBudgetModel(bucketRepo));
            return mapper.ToDto(TestData);
        }
    }
}
