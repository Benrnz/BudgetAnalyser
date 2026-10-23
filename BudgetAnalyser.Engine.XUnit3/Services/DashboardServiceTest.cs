using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class DashboardServiceTest
{
    private readonly IWidgetRepository mockWidgetRepo;
    private readonly DashboardService subject;

    public DashboardServiceTest(ITestOutputHelper writer)
    {
        var logger = new XUnitLogger(writer);
        var widgetService = new WidgetService(Substitute.For<IBudgetBucketRepository>(), new MonitorableDependencies(new LedgerCalculation(logger), logger), logger);
        this.mockWidgetRepo = Substitute.For<IWidgetRepository>();
        this.subject = new DashboardService(widgetService, logger, this.mockWidgetRepo, Substitute.For<IDirtyDataService>(), new WidgetCatalog());
    }

    [Fact]
    public void InitialiseWithDefaultWidgets_ShouldIncludeOpenNewAndDemoWidgets()
    {
        this.subject.InitialiseWithDefaultWidgets();

        var widgets = this.subject.WidgetsToDisplay().SelectMany(g => g.Widgets).ToList();
        widgets.ShouldContain(w => w is CurrentFileWidget);
        widgets.ShouldContain(w => w is NewFileWidget);
        widgets.ShouldContain(w => w is LoadDemoWidget);
    }

    [Fact]
    public void InitialiseWithDefaultWidgets_ShouldNotifyNewDataSourceAvailable()
    {
        var raised = false;
        this.subject.NewDataSourceAvailable += (_, _) => raised = true;

        this.subject.InitialiseWithDefaultWidgets();

        raised.ShouldBeTrue();
    }

    [Fact]
    public async Task InitialiseWithDefaultWidgets_ShouldNotReplaceWidgets_WhenFileAlreadyLoaded()
    {
        this.mockWidgetRepo.LoadAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns([new SaveWidget()]);
        await this.subject.LoadAsync(new ApplicationDatabase());

        this.subject.InitialiseWithDefaultWidgets();

        this.subject.WidgetsToDisplay().SelectMany(g => g.Widgets).Single().ShouldBeOfType<SaveWidget>();
    }
}
