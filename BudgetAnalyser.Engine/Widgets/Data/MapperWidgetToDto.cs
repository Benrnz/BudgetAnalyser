using System.Diagnostics;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Widgets.Data;

[AutoRegisterWithIoC]
public class MapperWidgetToDto(IStandardWidgetCatalog widgetCatalog) : IDtoMapper<WidgetDto, Widget>
{
    private readonly IStandardWidgetCatalog widgetCatalog = widgetCatalog ?? throw new ArgumentNullException(nameof(widgetCatalog));

    public WidgetDto ToDto(Widget model)
    {
        switch (model)
        {
            case SurprisePaymentWidget s:
                return new SurprisePaymentWidgetDto
                {
                    WidgetGroup = s.Category,
                    Visible = s.Visibility,
                    WidgetType = s.GetType().FullName!,
                    Frequency = s.Frequency,
                    BucketCode = s.BucketCode,
                    PaymentStartDate = DateOnly.FromDateTime(s.StartPaymentDate)
                };

            case BudgetBucketMonitorWidget b:
                return new MultiInstanceWidgetDto { WidgetGroup = b.Category, Visible = b.Visibility, WidgetType = b.GetType().FullName!, BucketCode = b.BucketCode };

            case FixedBudgetMonitorWidget f:
                return new MultiInstanceWidgetDto { WidgetGroup = f.Category, Visible = f.Visibility, WidgetType = f.GetType().FullName!, BucketCode = f.BucketCode };

            default:
                // CurrentFileWidget, DateFilterWidget, DaysSinceLastImport, DisusedMatchingRuleWidget, EncryptWidget, NewFileWidget, OverspentWarning, RemainingActualSurplusWidget,
                // RemainingSurplusWidget, SaveWidget, TimedUpdateCounterWidget, UpdateMobileDataWidget
                return new WidgetDto { Visible = model.Visibility, WidgetGroup = model.Category, WidgetType = model.GetType().FullName! };
        }
    }

    public Widget ToModel(WidgetDto dto)
    {
        Widget widget;
        if (dto is MultiInstanceWidgetDto m)
        {
            var type = Type.GetType(dto.WidgetType) ?? throw new DataFormatException($"The widget type specified {dto.WidgetType} is not found in any known type library.");

            var createdObject = Activator.CreateInstance(type);
            Debug.Assert(createdObject is not null);
            if (createdObject is IUserDefinedWidget multiInstanceWidget)
            {
                multiInstanceWidget.Id = m.BucketCode;
            }
            else
            {
                throw new DataFormatException($"The widget type specified {dto.WidgetType} is not a IUserDefinedWidget");
            }

            widget = (Widget)multiInstanceWidget;
        }
        else if (dto is SurprisePaymentWidgetDto s)
        {
            widget = new SurprisePaymentWidget { Frequency = s.Frequency, Id = s.BucketCode, StartPaymentDate = s.PaymentStartDate.ToDateTime(TimeOnly.MinValue) };
        }
        else
        {
            widget = this.widgetCatalog.FindWidget(dto.WidgetType) ??
                     throw new DataFormatException($"The standard widget type specified {dto.WidgetType} is not found in the standard widget catalog.");
        }

        widget.Visibility = dto.Visible;
        // widget.Category = dto.WidgetGroup; Currently this is not customisable - this is a possible future enhancement.

        return widget;
    }
}
