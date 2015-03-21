using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Persistence
{
    [TestClass]
    public class XamlOnDiskApplicationDatabaseRepositoryTest
    {
        private ApplicationDatabase result;
        private XamlOnDiskApplicationDatabaseRepositoryTestHarness subject;

        [TestInitialize()]
        public void TestInitialise()
        {
            this.subject = new XamlOnDiskApplicationDatabaseRepositoryTestHarness(new StorageRootToApplicationDatabaseMapper(), new ApplicationDatabaseToStorageRootMapper())
            {
                FileExistsOverride = fileName => true
            };

            var task = this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
            task.Wait();
            this.result = task.Result;
        }

        [TestMethod]
        public void LoadShouldSetBudgetCollectionStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoBudget.xml", this.result.BudgetCollectionStorageKey);
        }

        [TestMethod]
        public void LoadShouldSetStatementModelStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoTransactions.csv", this.result.StatementModelStorageKey);
        }

        [TestMethod]
        public void LoadShouldSetLedgerBookStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoLedgerBook.xml", this.result.LedgerBookStorageKey);
        }

        [TestMethod]
        public void LoadShouldSetMatchingRulesStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoMatchingRules.xml", this.result.MatchingRulesCollectionStorageKey);
        }

        [TestMethod]
        public void LoadShouldSetReconciliationTasksGivenDemoFile()
        {
            Assert.AreEqual(2, this.result.LedgerReconciliationToDoCollection.Count);
        }
    }
}
