#nullable enable

using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;


namespace BudgetAnalyser.Engine.XUnit.Services;

public class WidgetServiceTest
{
    private readonly ILogger logger;
    private readonly IBudgetBucketRepository mockBucketRepository;
    private readonly IMonitorableDependencies mockMonitoringServices;
    private readonly WidgetService service;

    public WidgetServiceTest(ITestOutputHelper writer)
    {
        this.mockBucketRepository = Substitute.For<IBudgetBucketRepository>();
        this.logger = new XUnitLogger(writer);
        this.mockMonitoringServices = Substitute.For<IMonitorableDependencies>();
        this.service = new WidgetService(this.mockBucketRepository, this.mockMonitoringServices, this.logger);
    }

    [Fact]
    public void Constructor_ShouldThrow_GivenNullBucketRepository()
    {
        Should.Throw<ArgumentNullException>(() => new WidgetService(null, this.mockMonitoringServices, this.logger));
    }

    [Fact]
    public void Constructor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new WidgetService(this.mockBucketRepository, this.mockMonitoringServices, null));
    }

    [Fact]
    public void Constructor_ShouldThrow_GivenNullMonitoringServices()
    {
        Should.Throw<ArgumentNullException>(() => new WidgetService(this.mockBucketRepository, null, this.logger));
    }

    [Fact]
    public void CreateFixedBudgetMonitorWidget_ShouldReturnWidget()
    {
        var bucket = new FixedBudgetProjectBucket("Bucket1", "Description", 100);
        this.mockBucketRepository.CreateNewFixedBudgetProject(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>()).Returns(bucket);
        this.mockBucketRepository.GetByCode(Arg.Any<string>()).Returns(bucket);

        var widget = this.service.CreateFixedBudgetMonitorWidget("Bucket1", "Description", 100);
        widget.ShouldNotBeNull();
    }

    [Fact]
    public void CreateNewSurprisePaymentWidget_ShouldReturnWidget()
    {
        this.mockBucketRepository.GetByCode(Arg.Any<string>()).Returns(StatementModelTestData.HairBucket);
        var widget = this.service.CreateNewSurprisePaymentWidget("Bucket1", DateOnlyExt.Today(), WeeklyOrFortnightly.Weekly);
        widget.ShouldNotBeNull();
    }

    [Fact]
    public void CreateUserDefinedWidget_ShouldReturnWidget()
    {
        this.mockBucketRepository.GetByCode(TestDataConstants.PhoneBucketCode).Returns(StatementModelTestData.PhoneBucket);
        var widget = this.service.CreateUserDefinedWidget(typeof(BudgetBucketMonitorWidget).FullName!, TestDataConstants.PhoneBucketCode);
        widget.ShouldNotBeNull();
    }

    [Fact]
    public void CreateUserDefinedWidget_ShouldThrow_GivenInvalidBucketCode()
    {
        Should.Throw<ArgumentException>(() => this.service.CreateUserDefinedWidget("TestWidget", "InvalidBucket"));
    }

    [Fact]
    public void Initialise_ShouldAddWidgetsToCache()
    {
        var widgets = new List<Widget> { new TestWidget("Widget1") };
        this.service.Initialise(widgets);

        var arrangedWidgets = this.service.ArrangeWidgetsForDisplay();
        arrangedWidgets.ShouldNotBeEmpty();
        arrangedWidgets.First().Widgets.ShouldContain(w => w.Name == "Widget1");
    }

    [Fact]
    public void Initialise_ShouldThrow_GivenEmptyWidgetsFromPersistence()
    {
        Should.Throw<ArgumentException>(() => this.service.Initialise(Array.Empty<Widget>()));
    }

    [Fact]
    public void Initialise_ShouldThrow_GivenNullWidgetsFromPersistence()
    {
        Should.Throw<ArgumentNullException>(() => this.service.Initialise(null));
    }

    [Fact]
    public void RemoveUserDefinedWidget_ShouldRemoveWidgetFromCache()
    {
        var widget1 = new TestUserDefinedWidget("Widget1", "FOOD");
        var widget2 = new TestUserDefinedWidget("Widget2", "POO");
        this.service.Initialise(new List<Widget> { widget1, widget2 });

        this.service.RemoveUserDefinedWidget(widget2);
        var arrangedWidgets = this.service.ArrangeWidgetsForDisplay();
        arrangedWidgets.First().Widgets.ShouldNotContain(w => w.Name == "Widget2");
    }

    [Fact]
    public void UpdateWidgetData_ShouldUpdateWidget()
    {
        var widget = new TestWidget(string.Empty);
        this.service.UpdateWidgetData(widget);
        widget.Updated.ShouldBeTrue();
    }

    private class TestWidget : Widget
    {
        public TestWidget(string name)
        {
            Category = WidgetGroup.OverviewSectionName;
            Name = name;
        }

        public bool Updated { get; private set; }

        public override void Update(params object[] input)
        {
            Updated = true;
        }
    }

    private class TestUserDefinedWidget(string name, string id) : TestWidget(name), IUserDefinedWidget
    {
        public string Id { get; set; } = id;
    }
}
