namespace BudgetAnalyser.Engine.Widgets.Data;

public record SurprisePaymentWidgetDto(bool Visible, string WidgetGroup, string WidgetType, string BucketCode, WeeklyOrFortnightly Frequency, DateOnly PaymentStartDate)
    : WidgetDto(Visible, WidgetGroup, WidgetType);
