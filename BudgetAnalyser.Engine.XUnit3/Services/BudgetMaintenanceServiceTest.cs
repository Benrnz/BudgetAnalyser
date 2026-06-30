using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class BudgetMaintenanceServiceTest
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly BudgetCollection budgetCollection;
    private readonly IBudgetRepository budgetRepo;
    private readonly ILogger logger;
    private readonly FakeMonitorableDependencies monitorableDependencies;
    private readonly BudgetMaintenanceService service;

    public BudgetMaintenanceServiceTest()
    {
        this.budgetRepo = Substitute.For<IBudgetRepository>();
        this.bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.logger = Substitute.For<ILogger>();
        this.monitorableDependencies = new FakeMonitorableDependencies();
        this.budgetCollection = new BudgetCollection(BudgetModelTestData.CreateTestData1());

        this.service = new BudgetMaintenanceService(
            this.budgetRepo,
            this.bucketRepo,
            this.logger,
            this.monitorableDependencies);

        PrivateAccessor.SetProperty(this.service, "Budgets", this.budgetCollection);
    }

    [Fact]
    public void CloneBudgetModel_ShouldAddToCollection()
    {
        var source = this.budgetCollection.CurrentActiveBudget;
        source.ShouldNotBeNull();

        (this.budgetCollection.Count == 1).ShouldBeTrue();
        this.service.CloneBudgetModel(source, DateOnlyExt.Today().AddDays(1), BudgetCycle.Monthly);
        (this.budgetCollection.Count == 2).ShouldBeTrue();
    }

    [Fact]
    public void CloneBudgetModel_WhenEffectiveDateIsPast_ShouldFail()
    {
        var source = this.budgetCollection.CurrentActiveBudget;
        source.ShouldNotBeNull();

        Should.Throw<ArgumentException>(() => this.service.CloneBudgetModel(source, DateOnlyExt.Today(), BudgetCycle.Monthly));
    }

    [Fact]
    public void CloneBudgetModel_WhenNewBudgetEffectiveFromIsBeforeOrEqualSourceBudgetEffectiveFrom_ThrowsArgumentException()
    {
        var sourceBudget = new BudgetModel { EffectiveFrom = DateOnlyExt.Today() };
        var newBudgetEffectiveFrom = DateOnlyExt.Today();
        var budgetCycle = BudgetCycle.Monthly;

        Should.Throw<ArgumentException>(() => this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle));
    }

    [Fact]
    public void CloneBudgetModel_WhenNewBudgetEffectiveFromIsLessThanOrEqualToToday_ThrowsArgumentException()
    {
        var sourceBudget = new BudgetModel { EffectiveFrom = DateOnlyExt.Today() };
        var newBudgetEffectiveFrom = DateOnlyExt.Today().AddDays(-1);
        var budgetCycle = BudgetCycle.Monthly;

        Should.Throw<ArgumentException>(() => this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle));
    }

    [Fact]
    public void CloneBudgetModel_WhenSourceBudgetIsEmpty_ShouldFail()
    {
        var source = new BudgetModel();

        Should.Throw<ArgumentException>(() => this.service.CloneBudgetModel(source, DateOnlyExt.Today(), BudgetCycle.Monthly));
    }

    [Fact]
    public void CloneBudgetModel_WhenSourceBudgetIsNull_ThrowsArgumentNullException()
    {
        BudgetModel sourceBudget = null!;
        var newBudgetEffectiveFrom = DateOnlyExt.Today().AddDays(1);
        var budgetCycle = BudgetCycle.Monthly;

        Should.Throw<ArgumentNullException>(() => this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle));
    }

    [Fact]
    public void CloneBudgetModel_WhenSourceCycleIsMonthly_ShouldBeAbleToSetClonedToFortnightly()
    {
        var source = this.budgetCollection.CurrentActiveBudget;
        source.ShouldNotBeNull();

        source.BudgetCycle = BudgetCycle.Monthly;
        var result = this.service.CloneBudgetModel(source, DateOnlyExt.Today().AddDays(1), BudgetCycle.Fortnightly);
        result.BudgetCycle.ShouldBe(BudgetCycle.Fortnightly);
    }

    [Fact]
    public void Constructor_WhenBudgetBucketRepositoryIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new BudgetMaintenanceService(this.budgetRepo, null!, this.logger, this.monitorableDependencies));
    }

    [Fact]
    public void Constructor_WhenBudgetRepositoryIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new BudgetMaintenanceService(null!, this.bucketRepo, this.logger, this.monitorableDependencies));
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new BudgetMaintenanceService(this.budgetRepo, this.bucketRepo, null!, this.monitorableDependencies));
    }

    [Fact]
    public void Constructor_WhenMonitorableDependenciesIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new BudgetMaintenanceService(this.budgetRepo, this.bucketRepo, this.logger, null!));
    }
}
