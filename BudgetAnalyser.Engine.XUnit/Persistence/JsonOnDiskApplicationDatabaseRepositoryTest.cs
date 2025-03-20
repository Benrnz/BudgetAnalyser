using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Persistence;

public class JsonOnDiskApplicationDatabaseRepositoryTest : IAsyncLifetime
{
    private readonly JsonOnDiskApplicationDatabaseRepositoryTestHarness subject;
    private ApplicationDatabase result;

    public JsonOnDiskApplicationDatabaseRepositoryTest(ITestOutputHelper outputHelper)
    {
        var logger = new XUnitLogger(outputHelper);
        this.subject = new JsonOnDiskApplicationDatabaseRepositoryTestHarness(new MapperApplicationDatabaseToStorageRoot2(), logger) { FileExistsOverride = fileName => true };
        // Cannot call async methods in constructor.
    }

    public async Task InitializeAsync()
    {
        this.result = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
    }

    public Task DisposeAsync()
    {
        // Cleanup if necessary
        return Task.CompletedTask;
    }

    [Fact]
    public void LoadShouldSetBudgetCollectionStorageKeyGivenDemoFile()
    {
        this.result.BudgetCollectionStorageKey.ShouldBe("DemoBudget.xml");
    }

    [Fact]
    public void LoadShouldSetLedgerBookStorageKeyGivenDemoFile()
    {
        this.result.LedgerBookStorageKey.ShouldBe("DemoLedgerBook.xml");
    }

    [Fact]
    public void LoadShouldSetMatchingRulesStorageKeyGivenDemoFile()
    {
        this.result.MatchingRulesCollectionStorageKey.ShouldBe("DemoMatchingRules.xml");
    }

    [Fact]
    public void LoadShouldSetReconciliationTasksGivenDemoFile()
    {
        this.result.LedgerReconciliationToDoCollection.Count.ShouldBe(2);
    }

    [Fact]
    public void LoadShouldSetStatementModelStorageKeyGivenDemoFile()
    {
        this.result.StatementModelStorageKey.ShouldBe("DemoTransactions.csv");
    }

    [Fact]
    public void LoadShouldSetWidgetStorageKeyGivenDemoFile()
    {
        this.result.WidgetsCollectionStorageKey.ShouldBe("Widgets.xml");
    }
}
