namespace BudgetAnalyser.Engine.Widgets.Data;

public class WidgetDto
{
    public required bool Visible { get; init; }
    public required string WidgetGroup { get; init; }
    public required string WidgetType { get; init; }
}

public class SurprisePaymentWidgetDto : WidgetDto
{
    public required string BucketCode { get; init; }
    public required WeeklyOrFortnightly Frequency { get; init; }
    public required DateOnly PaymentStartDate { get; init; }
}

public class MultiInstanceWidgetDto : WidgetDto
{
    public required string BucketCode { get; init; }
}
