using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.Widgets.Data;

[JsonDerivedType(typeof(WidgetDto), typeDiscriminator: "base")]
[JsonDerivedType(typeof(MultiInstanceWidgetDto), typeDiscriminator: "multi")]
[JsonDerivedType(typeof(SurprisePaymentWidgetDto), typeDiscriminator: "surprisepmt")]
public class WidgetDto
{
    public required bool Visible { get; init; }
    public required string WidgetGroup { get; init; }
    public required string WidgetType { get; init; }
}
