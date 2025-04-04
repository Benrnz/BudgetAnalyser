using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class MonitorableDependenciesTests
{
    private readonly LedgerCalculation mockLedgerCalculator;
    private readonly MonitorableDependencies service;

    public MonitorableDependenciesTests(ITestOutputHelper writer)
    {
        this.mockLedgerCalculator = Substitute.For<LedgerCalculation>();
        this.service = new MonitorableDependencies(this.mockLedgerCalculator, new XUnitLogger(writer));
    }

    [Fact]
    public void Constructor_ShouldInitializeDependencies()
    {
        var sut = new FakeMonitorableDependencies();
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(StatementModel));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(BudgetCollection));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(IBudgetCurrencyContext));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(LedgerBook));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(IBudgetBucketRepository));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(GlobalFilterCriteria));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(LedgerCalculation));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(ApplicationDatabase));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(ITransactionRuleService));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(ILogger));
        sut.SupportedWidgetDependencyTypes.ShouldContain(typeof(IDirtyDataService));
    }

    [Fact]
    public void NotifyOfDependencyChange_ShouldThrow_WhenDependencyIsInvalid()
    {
        Should.Throw<KeyNotFoundException>(() => this.service.NotifyOfDependencyChange(this));
    }

    [Fact]
    public void NotifyOfDependencyChange_ShouldReturnFalse_WhenDependencyIsNull()
    {
        var result = this.service.NotifyOfDependencyChange<StatementModel>(null);
        result.ShouldBeFalse();
    }

    [Fact]
    public void NotifyOfDependencyChange_ShouldReturnTrue_WhenDependencyHasChanged()
    {
        var criteria = new GlobalFilterCriteria();
        var result = this.service.NotifyOfDependencyChange(criteria);
        result.ShouldBeTrue();
    }

    [Fact]
    public void NotifyOfDependencyChange_ShouldTriggerEvent_WhenDependencyHasSignificantlyChanged()
    {
        var statementModel = TestData.StatementModelTestData.TestData1();
        var eventTriggered = false;
        this.service.DependencyChanged += (sender, args) => eventTriggered = true;

        this.service.NotifyOfDependencyChange(statementModel);

        eventTriggered.ShouldBeTrue();
    }

    [Fact]
    public void RetrieveDependency_ShouldReturnDependency_WhenKeyIsValid()
    {
        var result = this.service.RetrieveDependency(typeof(LedgerCalculation));
        result.ShouldBe(this.mockLedgerCalculator);
    }

    [Fact]
    public void RetrieveDependency_ShouldThrowException_WhenKeyIsInvalid()
    {
        Should.Throw<NotSupportedException>(() => this.service.RetrieveDependency(typeof(string)));
    }
}
