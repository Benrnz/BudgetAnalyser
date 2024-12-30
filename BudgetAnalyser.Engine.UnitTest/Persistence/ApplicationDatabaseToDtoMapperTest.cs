using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Persistence
{
    [TestClass]
    public class ApplicationDatabaseToDtoMapperTest
    {
        private BudgetAnalyserStorageRoot result;
        private ApplicationDatabase testData;

        [TestMethod]
        public void ShouldMapBudgetCollectionRootDto()
        {
            Assert.AreEqual("Budget.xml", this.result.BudgetCollectionRootDto.Source);
        }

        [TestMethod]
        public void ShouldMapLedgerBookRootDto()
        {
            Assert.AreEqual("Ledger.xml", this.result.LedgerBookRootDto.Source);
        }

        [TestMethod]
        public void ShouldMapLedgerReconciliationToDoCollection()
        {
            Assert.AreEqual(2, this.result.LedgerReconciliationToDoCollection.Count);
        }

        [TestMethod]
        public void ShouldMapMatchingRulesCollectionRootDto()
        {
            Assert.AreEqual("Rules.xml", this.result.MatchingRulesCollectionRootDto.Source);
        }

        [TestMethod]
        public void ShouldMapStatementModelRootDto()
        {
            Assert.AreEqual("Statement.xml", this.result.StatementModelRootDto.Source);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var todoCollection = new ToDoCollection
            {
                new ToDoTask("Foo1"),
                new ToDoTask("Foo2", false, false)
            };
            this.testData = new ApplicationDatabase();
            PrivateAccessor.SetProperty(this.testData, "BudgetCollectionStorageKey", "Budget.xml");
            PrivateAccessor.SetProperty(this.testData, "FileName", "C:\\Foo\\TestData.bax");
            PrivateAccessor.SetProperty(this.testData, "LedgerBookStorageKey", "Ledger.xml");
            PrivateAccessor.SetProperty(this.testData, "MatchingRulesCollectionStorageKey", "Rules.xml");
            PrivateAccessor.SetProperty(this.testData, "StatementModelStorageKey", "Statement.xml");
            PrivateAccessor.SetProperty(this.testData, "LedgerReconciliationToDoCollection", todoCollection);

            var subject = new MapperBudgetAnalyserStorageRoot2ApplicationDatabase();
            this.result = subject.ToDto(this.testData);
        }
    }
}
