using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;


namespace BudgetAnalyser.Engine.XUnit.Persistence;

public class JsonOnDiskApplicationDatabaseRepositoryTest : IDisposable
{
    private readonly XUnitLogger logger;
    private readonly JsonOnDiskApplicationDatabaseRepositoryTestHarness subject;
    private ApplicationDatabase? resultingModel;

    public JsonOnDiskApplicationDatabaseRepositoryTest(ITestOutputHelper outputHelper)
    {
        this.logger = new XUnitLogger(outputHelper);
        this.subject = new JsonOnDiskApplicationDatabaseRepositoryTestHarness(new MapperApplicationDatabaseToStorageRoot3(), this.logger) { FileExistsOverride = fileName => true };
        // Cannot call async methods in constructor.
    }

    public void Dispose()
    {
        this.subject.Dispose();
    }

    [Fact]
    public async Task CreateNew_ShouldCreateNewApplicationDatabaseModel()
    {
        this.resultingModel = await this.subject.CreateNewAsync("Test");
        this.resultingModel.ShouldNotBeNull();
        this.resultingModel.StatementModelStorageKey.ShouldBe("Test.Transactions.csv");
        this.resultingModel.BudgetCollectionStorageKey.ShouldBe("Test.Budget.json");
        this.resultingModel.LedgerBookStorageKey.ShouldBe("Test.LedgerBook.json");
        this.resultingModel.WidgetsCollectionStorageKey.ShouldBe("Test.Widgets.json");
        this.resultingModel.MatchingRulesCollectionStorageKey.ShouldBe("Test.MatchingRules.json");
        this.resultingModel.IsEncrypted.ShouldBeFalse();
        this.resultingModel.FileName.ShouldBe("Test");
    }

    [Fact]
    public async Task CreateNew_ShouldSaveNewApplicationDatabaseModel()
    {
        this.resultingModel = await this.subject.CreateNewAsync("Test");
        this.subject.SerialisedData.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateNew_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.CreateNewAsync(null!));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new JsonOnDiskApplicationDatabaseRepository(new MapperApplicationDatabaseToStorageRoot3(), null!));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() => new JsonOnDiskApplicationDatabaseRepository(null!, this.logger));
    }

    [Fact]
    public async Task Load_ShouldSetBudgetCollectionStorageKey_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.BudgetCollectionStorageKey.ShouldBe("DemoBudget.json");
    }

    [Fact]
    public async Task Load_ShouldSetLedgerBookStorageKey_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.LedgerBookStorageKey.ShouldBe("DemoLedgerBook.json");
    }

    [Fact]
    public async Task Load_ShouldSetMatchingRulesStorageKey_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.MatchingRulesCollectionStorageKey.ShouldBe("DemoMatchingRules.json");
    }

    [Fact]
    public async Task Load_ShouldSetReconciliationTasks_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.LedgerReconciliationToDoCollection.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Load_ShouldSetStatementModelStorageKey_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.StatementModelStorageKey.ShouldBe("DemoTransactions.csv");
    }

    [Fact]
    public async Task Load_ShouldSetWidgetStorageKey_GivenDemoFile()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        this.resultingModel.WidgetsCollectionStorageKey.ShouldBe("Widgets.json");
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenNonExistentFile()
    {
        this.subject.FileExistsOverride = fileName => false;
        await Should.ThrowAsync<KeyNotFoundException>(() => this.subject.LoadAsync("NonExistentFile"));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentException>(() => this.subject.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAndSave_ShouldResultInSameJson()
    {
        this.resultingModel = await this.subject.LoadAsync(TestDataConstants.DemoBudgetAnalyserFileName);
        var initialJson = JsonHelper.MinifyJson(this.subject.SerialisedData);
        await this.subject.SaveAsync(this.resultingModel);
        var resavedJson = JsonHelper.MinifyJson(this.subject.SerialisedData);

        resavedJson.ShouldBe(initialJson);
    }

    [Fact]
    public async Task Save_ShouldSaveApplicationDatabaseModel()
    {
        this.resultingModel = new ApplicationDatabase
        {
            BudgetCollectionStorageKey = "Test.Budget.json",
            LedgerBookStorageKey = "Test.LedgerBook.json",
            StatementModelStorageKey = "Test.Transactions.csv",
            LedgerReconciliationToDoCollection = new ToDoCollection(),
            MatchingRulesCollectionStorageKey = "Test.MatchingRules.json",
            WidgetsCollectionStorageKey = "Test.Widgets.json",
            IsEncrypted = false,
            FileName = "Test.bax"
        };
        await this.subject.SaveAsync(this.resultingModel);
        this.subject.SerialisedData.ShouldNotBeNull();
        this.subject.SerialisedData.Length.ShouldBeGreaterThan(330);
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNullModel()
    {
        await Should.ThrowAsync<ArgumentNullException>(() => this.subject.SaveAsync(null!));
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentException>(() => this.subject.SaveAsync(new ApplicationDatabase()));
    }
}
