using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BudgetAnalyser.Engine.Widgets
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReflectionWidgetRepository : IWidgetRepository
    {
        private readonly SortedList<string, Widget> cachedWidgets;

        public ReflectionWidgetRepository()
        {
            cachedWidgets = new SortedList<string, Widget>();
        }

        public IMultiInstanceWidget Create(string widgetType, string id)
        {
            var type = Type.GetType(widgetType);
            if (type == null)
            {
                throw new NotSupportedException("The widget type specified " + widgetType + " is not found in any known type library.");
            }

            if (!typeof(IMultiInstanceWidget).IsAssignableFrom(type))
            {
                throw new NotSupportedException("The widget type specified " + widgetType + " is not a IMultiInstanceWidget");
            }

            var widget = Activator.CreateInstance(type) as IMultiInstanceWidget;
            Debug.Assert(widget != null);
            widget.Id = id;
            var baseWidget = (Widget)widget;
            this.cachedWidgets.Add(baseWidget.Category + baseWidget.Name + widget.Id, baseWidget);
            return widget;
        }

        public IEnumerable<Widget> GetAll()
        {
            if (!this.cachedWidgets.Any())
            {
                IEnumerable<Type> widgetTypes = GetType().Assembly.GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var widget in widgetTypes
                    .Where(t => !typeof(IMultiInstanceWidget).IsAssignableFrom(t))
                    .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
                {
                    this.cachedWidgets.Add(widget.Category + widget.Name, widget);
                }
            }

            return this.cachedWidgets.Values;
        }
    }
}