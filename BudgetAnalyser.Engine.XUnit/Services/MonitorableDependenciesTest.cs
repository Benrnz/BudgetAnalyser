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
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(StatementModel));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(BudgetCollection));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(IBudgetCurrencyContext));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(LedgerBook));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(IBudgetBucketRepository));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(GlobalFilterCriteria));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(LedgerCalculation));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(ApplicationDatabase));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(ITransactionRuleService));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(IApplicationDatabaseService));
        this.service.SupportedWidgetDependencyTypes.ShouldContain(typeof(ILogger));
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
        var statementModel = Substitute.For<IDataChangeDetection>();
        statementModel.SignificantDataChangeHash().Returns(1L);
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
