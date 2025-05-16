using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.Widgets.Data;

[JsonDerivedType(typeof(WidgetDto), "base")]
[JsonDerivedType(typeof(MultiInstanceWidgetDto), "multi")]
[JsonDerivedType(typeof(SurprisePaymentWidgetDto), "surprisepmt")]
public record WidgetDto(bool Visible, string WidgetGroup, string WidgetType);
