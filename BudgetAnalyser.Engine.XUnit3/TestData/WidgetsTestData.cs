using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;

namespace BudgetAnalyser.Engine.XUnit.TestData;

public static class WidgetsTestData
{
    public static IEnumerable<Widget> ModelTestData1()
    {
        return new List<Widget>
        {
            new EncryptWidget { Visibility = true },
            new OverspentWarning { Visibility = true },
            new SaveWidget { Visibility = true },
            new CurrentFileWidget { Visibility = true },
            new DateFilterWidget { Visibility = true },
            new NewFileWidget { Visibility = true },
            new RemainingSurplusWidget { Visibility = false },
            new SurprisePaymentWidget { Visibility = true },
            new BudgetBucketMonitorWidget { Visibility = true },
            new DaysSinceLastImport { Visibility = true },
            new DisusedMatchingRuleWidget { Visibility = true },
            new FixedBudgetMonitorWidget { Visibility = true },
            new RemainingActualSurplusWidget { Visibility = true },
            new TimedUpdateCounterWidget { Visibility = false },
            new UpdateMobileDataWidget { Visibility = true },
            new SurprisePaymentWidget { Visibility = true, Frequency = WeeklyOrFortnightly.Fortnightly, Id = TestDataConstants.RentBucketCode, StartPaymentDate = new DateOnly(2013, 7, 1) },
            new BudgetBucketMonitorWidget { Visibility = true, BucketCode = TestDataConstants.FoodBucketCode },
            new BudgetBucketMonitorWidget { Visibility = true, BucketCode = TestDataConstants.HairBucketCode },
            new FixedBudgetMonitorWidget { Visibility = false, BucketCode = "SURPLUS.FENCE" },
            new FixedBudgetMonitorWidget { Visibility = true, BucketCode = "SURPLUS.KITCHEN" }
        };
    }

    public static IEnumerable<WidgetDto> RawDtoTestData1()
    {
        return new List<WidgetDto>
        {
            new(WidgetType: typeof(EncryptWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(OverspentWarning).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(SaveWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(CurrentFileWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(DateFilterWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.GlobalFilterSectionName),
            new(WidgetType: typeof(NewFileWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(RemainingSurplusWidget).FullName!, Visible: false, WidgetGroup: WidgetGroup.PeriodicTrackingSectionName),
            new SurprisePaymentWidgetDto
            (
                WidgetType: typeof(SurprisePaymentWidget).FullName!,
                Visible: true,
                WidgetGroup: WidgetGroup.OverviewSectionName,
                Frequency: WeeklyOrFortnightly.Fortnightly,
                BucketCode: TestDataConstants.RentBucketCode,
                PaymentStartDate: new DateOnly(2013, 7, 1)
            ),
            new MultiInstanceWidgetDto
            (
                WidgetType: typeof(BudgetBucketMonitorWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.PeriodicTrackingSectionName, BucketCode: TestDataConstants.FoodBucketCode
            ),
            new MultiInstanceWidgetDto
            (
                WidgetType: typeof(BudgetBucketMonitorWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.PeriodicTrackingSectionName, BucketCode: TestDataConstants.HairBucketCode
            ),
            new(WidgetType: typeof(DaysSinceLastImport).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(DisusedMatchingRuleWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName),
            new MultiInstanceWidgetDto(WidgetType: typeof(FixedBudgetMonitorWidget).FullName!, Visible: false, WidgetGroup: WidgetGroup.ProjectsSectionName, BucketCode: "SURPLUS.FENCE"),
            new MultiInstanceWidgetDto(WidgetType: typeof(FixedBudgetMonitorWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.ProjectsSectionName, BucketCode: "SURPLUS.KITCHEN"),
            new(WidgetType: typeof(RemainingActualSurplusWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.PeriodicTrackingSectionName),
            new(WidgetType: typeof(TimedUpdateCounterWidget).FullName!, Visible: false, WidgetGroup: WidgetGroup.OverviewSectionName),
            new(WidgetType: typeof(UpdateMobileDataWidget).FullName!, Visible: true, WidgetGroup: WidgetGroup.OverviewSectionName)
        };
    }
}
