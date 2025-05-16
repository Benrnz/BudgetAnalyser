using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Persistence;

public class DtoToApplicationDatabaseMapperTest
{
    private readonly ApplicationDatabase result;
    private readonly BudgetAnalyserStorageRoot2 testData;

    public DtoToApplicationDatabaseMapperTest()
    {
        this.testData = new BudgetAnalyserStorageRoot2
        {
            BudgetCollectionRootDto = "Budget.xml",
            LedgerBookRootDto = "Ledger.xml",
            MatchingRulesCollectionRootDto = "Rules.xml",
            StatementModelRootDto = "Statement.xml",
            WidgetCollectionRootDto = "Widgets.xml",
            Filter = new GlobalFilterDto(),
            LedgerReconciliationToDoCollection = [new ToDoTaskDto(true, "Foo1", false), new ToDoTaskDto(false, "Foo2", true)]
        };

        var subject = new MapperApplicationDatabaseToStorageRoot3();
        this.result = subject.ToModel(this.testData);
    }

    [Fact]
    public void ShouldMapBudgetCollectionRootDto()
    {
        this.result.BudgetCollectionStorageKey.ShouldBe("Budget.xml");
    }

    [Fact]
    public void ShouldMapLedgerBookRootDto()
    {
        this.result.LedgerBookStorageKey.ShouldBe("Ledger.xml");
    }

    [Fact]
    public void ShouldMapLedgerReconciliationToDoCollection()
    {
        this.result.LedgerReconciliationToDoCollection.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldMapMatchingRulesCollectionRootDto()
    {
        this.result.MatchingRulesCollectionStorageKey.ShouldBe("Rules.xml");
    }

    [Fact]
    public void ShouldMapStatementModelRootDto()
    {
        this.result.StatementModelStorageKey.ShouldBe("Statement.xml");
    }
}
