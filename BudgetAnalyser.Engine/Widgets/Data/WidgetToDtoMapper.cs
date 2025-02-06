using System.Diagnostics;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Widgets.Data;

public class WidgetToDtoMapper(IWidgetRepository widgetRepository) : IDtoMapper<WidgetDto, Widget>
{
    private readonly IWidgetRepository widgetRepository = widgetRepository ?? throw new ArgumentNullException(nameof(widgetRepository));

    public WidgetDto ToDto(Widget model)
    {
        switch (model)
        {
            case SurprisePaymentWidget s:
                return new SurprisePaymentWidgetDto(
                    s.Category,
                    s.Visibility,
                    s.GetType().FullName!,
                    s.Frequency,
                    s.BucketCode,
                    DateOnly.FromDateTime(s.StartPaymentDate));

            case BudgetBucketMonitorWidget b:
                return new MultiInstanceWidgetDto(
                    b.Category,
                    b.Visibility,
                    b.GetType().FullName!,
                    b.BucketCode);

            case FixedBudgetMonitorWidget f:
                return new MultiInstanceWidgetDto(
                    f.Category,
                    f.Visibility,
                    f.GetType().FullName!,
                    f.BucketCode);


            default:
                // CurrentFileWidget, DateFilterWidget, DaysSinceLastImport, DisusedMatchingRuleWidget, EncryptWidget, NewFileWidget, OverspentWarning, RemainingActualSurplusWidget,
                // RemainingSurplusWidget, SaveWidget, TimedUpdateCounterWidget, UpdateMobileDataWidget
                return new WidgetDto(model.Category, model.Visibility, model.GetType().FullName!);
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
            widget = this.widgetRepository.GetAll().Single(w => w.GetType().FullName == dto.WidgetType);
        }

        widget.Visibility = dto.Visible;
        // widget.Category = dto.WidgetGroup; Currently this is not customisable - this is a possible future enhancement.

        return widget;
    }
}
