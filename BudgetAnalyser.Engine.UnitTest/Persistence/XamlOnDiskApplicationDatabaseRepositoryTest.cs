using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Persistence
{
    [TestClass]
    public class XamlOnDiskApplicationDatabaseRepositoryTest
    {
        private ApplicationDatabase result;
        private XamlOnDiskApplicationDatabaseRepositoryTestHarness subject;

        [TestMethod]
        public void LoadShouldSetBudgetCollectionStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoBudget.xml", this.result.BudgetCollectionStorageKey);
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

        [TestMethod]
        public void LoadShouldSetStatementModelStorageKeyGivenDemoFile()
        {
            Assert.AreEqual("DemoTransactions.csv", this.result.StatementModelStorageKey);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new XamlOnDiskApplicationDatabaseRepositoryTestHarness(new Mapper_BudgetAnalyserStorageRoot_ApplicationDatabase())
            {
                FileExistsOverride = fileName => true
            };

            Task<ApplicationDatabase> task = this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
            task.Wait();
            this.result = task.Result;
        }
    }
}