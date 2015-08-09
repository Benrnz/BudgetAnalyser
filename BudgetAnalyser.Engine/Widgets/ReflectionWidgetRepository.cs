using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

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

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IUserDefinedWidget")]
        public IUserDefinedWidget Create(string widgetType, string id)
        {
            Type type = Type.GetType(widgetType);
            if (type == null)
            {
                throw new NotSupportedException("The widget type specified " + widgetType + " is not found in any known type library.");
            }

            if (!typeof(IUserDefinedWidget).IsAssignableFrom(type))
            {
                throw new NotSupportedException("The widget type specified " + widgetType + " is not a IUserDefinedWidget");
            }

            var widget = Activator.CreateInstance(type) as IUserDefinedWidget;
            Debug.Assert(widget != null);
            widget.Id = id;
            string key = BuildMultiUseWidgetKey(widget);

            if (this.cachedWidgets.ContainsKey(key))
            {
                throw new ArgumentException("A widget with this key already exists.", nameof(id));
            }

            var baseWidget = (Widget)widget;
            this.cachedWidgets.Add(key, baseWidget);
            return widget;
        }

        public IEnumerable<Widget> GetAll()
        {
            if (this.cachedWidgets.None())
            {
                List<Type> widgetTypes = GetType().Assembly.GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

                List<Type> specialisedUiWidgets = Assembly.GetEntryAssembly().GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();
                if (specialisedUiWidgets.Any())
                {
                    widgetTypes.AddRange(specialisedUiWidgets);
                }

                foreach (Widget widget in widgetTypes
                    .Where(t => !typeof(IUserDefinedWidget).IsAssignableFrom(t))
                    .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
                {
                    this.cachedWidgets.Add(widget.Category + widget.Name, widget);
                }
            }

            return this.cachedWidgets.Values;
        }

        public void Remove(IUserDefinedWidget widget)
        {
            if (widget == null)
            {
                return;
            }

            this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget));
        }

        private static string BuildMultiUseWidgetKey(IUserDefinedWidget widget)
        {
            var baseWidget = (Widget)widget;
            return baseWidget.Category + baseWidget.Name + widget.Id;
        }
    }
}