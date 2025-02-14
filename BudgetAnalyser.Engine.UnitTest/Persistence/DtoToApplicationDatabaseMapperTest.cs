using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.UnitTest.Persistence;

[TestClass]
public class DtoToApplicationDatabaseMapperTest
{
    private ApplicationDatabase result;
    private BudgetAnalyserStorageRoot testData;

    [TestMethod]
    public void ShouldMapBudgetCollectionRootDto()
    {
        Assert.AreEqual("Budget.xml", this.result.BudgetCollectionStorageKey);
    }

    [TestMethod]
    public void ShouldMapLedgerBookRootDto()
    {
        Assert.AreEqual("Ledger.xml", this.result.LedgerBookStorageKey);
    }

    [TestMethod]
    public void ShouldMapLedgerReconciliationToDoCollection()
    {
        Assert.AreEqual(2, this.result.LedgerReconciliationToDoCollection.Count);
    }

    [TestMethod]
    public void ShouldMapMatchingRulesCollectionRootDto()
    {
        Assert.AreEqual("Rules.xml", this.result.MatchingRulesCollectionStorageKey);
    }

    [TestMethod]
    public void ShouldMapStatementModelRootDto()
    {
        Assert.AreEqual("Statement.xml", this.result.StatementModelStorageKey);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.testData = new BudgetAnalyserStorageRoot
        {
            BudgetCollectionRootDto = new StorageBranch { Source = "Budget.xml" },
            LedgerBookRootDto = new StorageBranch { Source = "Ledger.xml" },
            MatchingRulesCollectionRootDto = new StorageBranch { Source = "Rules.xml" },
            StatementModelRootDto = new StorageBranch { Source = "Statement.xml" },
            WidgetCollectionRootDto = new StorageBranch { Source = "Widgets.xml" },
            LedgerReconciliationToDoCollection = new List<ToDoTaskDto>
            {
                new() { CanDelete = true, Description = "Foo1", SystemGenerated = false }, new() { CanDelete = false, Description = "Foo2", SystemGenerated = true }
            }
        };

        var subject = new MapperApplicationDatabaseToStorageRoot2();
        this.result = subject.ToModel(this.testData);
    }
}
