using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Persistence;

public class ApplicationDatabaseToDtoMapperTest
{
    private readonly BudgetAnalyserStorageRoot2 result;
    private readonly ApplicationDatabase testData;

    public ApplicationDatabaseToDtoMapperTest()
    {
        var todoCollection = new ToDoCollection { new ToDoTask { Description = "Foo1" }, new ToDoTask { Description = "Foo2", CanDelete = false, SystemGenerated = false } };
        this.testData = new ApplicationDatabase();
        PrivateAccessor.SetProperty(this.testData, "BudgetCollectionStorageKey", "Budget.xml");
        PrivateAccessor.SetProperty(this.testData, "FileName", "C:\\Foo\\TestData.bax");
        PrivateAccessor.SetProperty(this.testData, "LedgerBookStorageKey", "Ledger.xml");
        PrivateAccessor.SetProperty(this.testData, "MatchingRulesCollectionStorageKey", "Rules.xml");
        PrivateAccessor.SetProperty(this.testData, "TransactionsSetModelStorageKey", "Statement.xml");
        PrivateAccessor.SetProperty(this.testData, "LedgerReconciliationToDoCollection", todoCollection);

        var subject = new MapperApplicationDatabaseToStorageRoot3();
        this.result = subject.ToDto(this.testData);
    }

    [Fact]
    public void ShouldMapBudgetCollectionRootDto()
    {
        this.result.BudgetCollectionRootDto.ShouldBe("Budget.xml");
    }

    [Fact]
    public void ShouldMapLedgerBookRootDto()
    {
        this.result.LedgerBookRootDto.ShouldBe("Ledger.xml");
    }

    [Fact]
    public void ShouldMapLedgerReconciliationToDoCollection()
    {
        this.result.LedgerReconciliationToDoCollection.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldMapMatchingRulesCollectionRootDto()
    {
        this.result.MatchingRulesCollectionRootDto.ShouldBe("Rules.xml");
    }

    [Fact]
    public void ShouldMapStatementModelRootDto()
    {
        this.result.StatementModelRootDto.ShouldBe("Statement.xml");
    }
}
