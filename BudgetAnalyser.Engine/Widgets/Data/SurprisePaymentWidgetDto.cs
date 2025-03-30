namespace BudgetAnalyser.Engine.Widgets.Data;

public class SurprisePaymentWidgetDto : WidgetDto
{
    public required string BucketCode { get; init; }
    public required WeeklyOrFortnightly Frequency { get; init; }
    public required DateOnly PaymentStartDate { get; init; }
}
