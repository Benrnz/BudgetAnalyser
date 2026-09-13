namespace BudgetAnalyser.Engine.Widgets.Data;

public record MultiInstanceWidgetDto(bool Visible, string WidgetGroup, string WidgetType, string BucketCode) : WidgetDto(Visible, WidgetGroup, WidgetType);
