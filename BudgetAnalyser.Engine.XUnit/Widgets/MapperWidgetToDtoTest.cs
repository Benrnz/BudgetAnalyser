using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class MapperWidgetToDtoTest
{
    private readonly IWidgetRepository mockWidgetRepo = Substitute.For<IWidgetRepository>();
    private readonly WidgetToDtoMapper subject;

    public MapperWidgetToDtoTest()
    {
        this.subject = new WidgetToDtoMapper(this.mockWidgetRepo);
    }

    public static IEnumerable<object[]> AllStandardWidgetDtos()
    {
        yield return [new WidgetDto(WidgetGroup.OverviewSectionName, true, typeof(CurrentFileWidget).FullName!), true, typeof(CurrentFileWidget)];
        yield return
        [
            new MultiInstanceWidgetDto(
                WidgetGroup.PeriodicTrackingSectionName,
                false,
                typeof(BudgetBucketMonitorWidget).FullName!,
                TestDataConstants.FoodBucketCode),
            false,
            typeof(BudgetBucketMonitorWidget)
        ];
        yield return
        [
            new MultiInstanceWidgetDto(
                WidgetGroup.ProjectsSectionName,
                true,
                typeof(FixedBudgetMonitorWidget).FullName!,
                "SURPLUS.SPEC2"),
            true,
            typeof(FixedBudgetMonitorWidget)
        ];
        yield return
        [
            new SurprisePaymentWidgetDto(
                WidgetGroup.ProjectsSectionName,
                true,
                typeof(SurprisePaymentWidget).FullName!,
                WeeklyOrFortnightly.Fortnightly,
                TestDataConstants.FoodBucketCode,
                new DateOnly(2013, 8, 1)),
            true,
            typeof(SurprisePaymentWidget)
        ];
    }

    public static IEnumerable<object[]> AllStandardWidgets()
    {
        yield return [new CurrentFileWidget { Visibility = false }, false];
        yield return [new SaveWidget { Visibility = true }, true];
        yield return [new TimedUpdateCounterWidget { Visibility = false }, false];
        yield return [new DaysSinceLastImport { Visibility = false }, false];
        yield return [new DisusedMatchingRuleWidget { Visibility = true }, true];
        yield return [new EncryptWidget { Visibility = false }, false];
        yield return [new NewFileWidget { Visibility = true }, true];
        yield return [new DateFilterWidget { Visibility = true }, true, WidgetGroup.GlobalFilterSectionName];
        yield return [new OverspentWarning { Visibility = false }, false, WidgetGroup.PeriodicTrackingSectionName];
        yield return [new RemainingActualSurplusWidget { Visibility = true }, true, WidgetGroup.PeriodicTrackingSectionName];
        yield return [new RemainingSurplusWidget { Visibility = false }, false, WidgetGroup.PeriodicTrackingSectionName];
        yield return [new UpdateMobileDataWidget { Visibility = true }, true, WidgetGroup.PeriodicTrackingSectionName];
    }

    [Fact]
    public void MapToBudgetBucketMonitorWidgetDto()
    {
        var model = new BudgetBucketMonitorWidget { Visibility = false, BucketCode = TestDataConstants.DoctorBucketCode, Value = 12.45, Clickable = true };
        var dto = this.subject.ToDto(model);

        dto.ShouldBeOfType<MultiInstanceWidgetDto>();
        var surpriseDto = (MultiInstanceWidgetDto)dto;

        var expected = new MultiInstanceWidgetDto(
            WidgetGroup.PeriodicTrackingSectionName,
            false,
            "BudgetAnalyser.Engine.Widgets.BudgetBucketMonitorWidget",
            TestDataConstants.DoctorBucketCode);

        surpriseDto.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void MapToFixedBudgetMonitorWidgetDto()
    {
        var model = new FixedBudgetMonitorWidget { Id = "SURPLUS.SPEC1", Visibility = true };
        var dto = this.subject.ToDto(model);

        dto.ShouldBeOfType<MultiInstanceWidgetDto>();
        var surpriseDto = (MultiInstanceWidgetDto)dto;

        var expected = new MultiInstanceWidgetDto(
            WidgetGroup.ProjectsSectionName,
            true,
            "BudgetAnalyser.Engine.Widgets.FixedBudgetMonitorWidget",
            "SURPLUS.SPEC1");

        surpriseDto.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void MapToSurprisePaymentWidgetDto()
    {
        var model = new SurprisePaymentWidget { Id = TestDataConstants.FoodBucketCode, StartPaymentDate = new DateTime(2013, 7, 1), Frequency = WeeklyOrFortnightly.Fortnightly, Visibility = false };
        var dto = this.subject.ToDto(model);

        dto.ShouldBeOfType<SurprisePaymentWidgetDto>();
        var surpriseDto = (SurprisePaymentWidgetDto)dto;

        var expected = new SurprisePaymentWidgetDto(
            WidgetGroup.OverviewSectionName,
            false,
            "BudgetAnalyser.Engine.Widgets.SurprisePaymentWidget",
            WeeklyOrFortnightly.Fortnightly,
            TestDataConstants.FoodBucketCode,
            new DateOnly(2013, 7, 1));

        surpriseDto.ShouldBeEquivalentTo(expected);
    }

    [MemberData(nameof(AllStandardWidgets))]
    [Theory]
    public void MapToWidgetDto(Widget model, bool expectedVisibility, string expectedCategory = WidgetGroup.OverviewSectionName)
    {
        // Standard Widgets that can't really have any persisted state aside from Visibility.
        // CurrentFileWidget, DateFilterWidget, DaysSinceLastImport, DisusedMatchingRuleWidget, EncryptWidget, NewFileWidget, OverspentWarning, RemainingActualSurplusWidget,
        // RemainingSurplusWidget, SaveWidget, TimedUpdateCounterWidget, UpdateMobileDataWidget

        var dto = this.subject.ToDto(model);

        dto.ShouldBeOfType<WidgetDto>();

        var expected = new WidgetDto(
            expectedCategory,
            expectedVisibility,
            model.GetType().FullName!);

        dto.ShouldBeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(AllStandardWidgetDtos))]
    public void MapToWidgetModel(WidgetDto dto, bool expectedVisibility, Type expectedType)
    {
        this.mockWidgetRepo.GetAll().Returns([new CurrentFileWidget()]);
        var model = this.subject.ToModel(dto);
        model.ShouldBeOfType(expectedType);
        model.Visibility.ShouldBe(expectedVisibility);
    }
}
