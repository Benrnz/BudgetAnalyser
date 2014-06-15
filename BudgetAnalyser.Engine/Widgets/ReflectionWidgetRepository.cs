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
            this.cachedWidgets = new SortedList<string, Widget>();
        }

        public event EventHandler<WidgetRepositoryChangedEventArgs> WidgetRemoved;

        public IMultiInstanceWidget Create(string widgetType, string id)
        {
            Type type = Type.GetType(widgetType);
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
            this.cachedWidgets.Add(BuildMultiUseWidgetKey(widget), baseWidget);
            return widget;
        }

        public IEnumerable<Widget> GetAll()
        {
            if (!this.cachedWidgets.Any())
            {
                IEnumerable<Type> widgetTypes = GetType().Assembly.GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (Widget widget in widgetTypes
                    .Where(t => !typeof(IMultiInstanceWidget).IsAssignableFrom(t))
                    .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
                {
                    this.cachedWidgets.Add(widget.Category + widget.Name, widget);
                }
            }

            return this.cachedWidgets.Values;
        }

        public void Remove(IMultiInstanceWidget widget)
        {
            if (widget == null)
            {
                return;
            }

            if (this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget)))
            {
                var handler = WidgetRemoved;
                if (handler != null)
                {
                    handler(this, new WidgetRepositoryChangedEventArgs((Widget)widget));
                }
            }
        }

        private static string BuildMultiUseWidgetKey(IMultiInstanceWidget widget)
        {
            var baseWidget = (Widget)widget;
            return baseWidget.Category + baseWidget.Name + widget.Id;
        }
    }
}