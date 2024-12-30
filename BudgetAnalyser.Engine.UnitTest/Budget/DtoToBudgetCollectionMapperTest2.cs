using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class DtoToBudgetCollectionMapperTest2
    {
        private BudgetCollection Result { get; set; }
        private BudgetCollectionDto TestData { get; set; }

        [TestMethod]
        public void ShouldMapBudgetsCorrectly()
        {
            Assert.AreEqual(TestData.Budgets.Sum(b => b.Expenses.Sum(e => e.Amount)), Result.Sum(b => b.Expenses.Sum(e => e.Amount)));
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.StorageKey, Result.StorageKey);
        }

        [TestMethod]
        public void ShouldMapSameNumberOfBudgets()
        {
            Assert.AreEqual(TestData.Budgets.Count, Result.Count);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = GetType().Assembly.ExtractEmbeddedResourceAsXamlObject<BudgetCollectionDto>("BudgetAnalyser.Engine.UnitTest.TestData.BudgetCollectionTestData.xml");
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var subject = new MapperBudgetCollectionDtoBudgetCollection(
                bucketRepo,
                new MapperBudgetBucketDtoBudgetBucket(new BudgetBucketFactory()),
                new MapperBudgetModelDtoBudgetModel(bucketRepo));
            Result = subject.ToModel(TestData);
        }
    }
}
