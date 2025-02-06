namespace BudgetAnalyser.Engine.Widgets.Data;

public record WidgetDto(string WidgetGroup, bool Visible, string WidgetType);

public record SurprisePaymentWidgetDto(string WidgetGroup, bool Visible, string WidgetType, WeeklyOrFortnightly Frequency, string BucketCode, DateOnly PaymentStartDate)
    : WidgetDto(WidgetGroup, Visible, WidgetType);

public record MultiInstanceWidgetDto(string WidgetGroup, bool Visible, string WidgetType, string BucketCode) : WidgetDto(WidgetGroup, Visible, WidgetType);
