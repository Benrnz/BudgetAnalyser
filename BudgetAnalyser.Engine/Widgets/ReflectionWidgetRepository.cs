using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A repository for widgets based on reflecting across available types in the attached assemblies.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.IWidgetRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReflectionWidgetRepository : IWidgetRepository
    {
        private readonly SortedList<string, Widget> cachedWidgets;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReflectionWidgetRepository" /> class.
        /// </summary>
        public ReflectionWidgetRepository()
        {
            this.cachedWidgets = new SortedList<string, Widget>();
        }

        /// <summary>
        ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
        ///     These can only be created after receiving the application state.
        /// </summary>
        /// <param name="widgetType">The full type name of the widget type.</param>
        /// <param name="id">A unique identifier for the instance</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">
        ///     The widget type specified  + widgetType +  is not found in any known type library.
        ///     or
        ///     The widget type specified  + widgetType +  is not a IUserDefinedWidget
        /// </exception>
        /// <exception cref="System.ArgumentException">A widget with this key already exists.</exception>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "IUserDefinedWidget")]
        public IUserDefinedWidget Create(string widgetType, string id)
        {
            var type = Type.GetType(widgetType);
            if (type == null)
            {
                throw new NotSupportedException("The widget type specified " + widgetType +
                                                " is not found in any known type library.");
            }

            if (!typeof(IUserDefinedWidget).IsAssignableFrom(type))
            {
                throw new NotSupportedException("The widget type specified " + widgetType +
                                                " is not a IUserDefinedWidget");
            }

            var widget = Activator.CreateInstance(type) as IUserDefinedWidget;
            Debug.Assert(widget != null);
            widget.Id = id;
            var key = BuildMultiUseWidgetKey(widget);

            if (this.cachedWidgets.ContainsKey(key))
            {
                throw new ArgumentException("A widget with this key already exists.", nameof(id));
            }

            var baseWidget = (Widget) widget;
            this.cachedWidgets.Add(key, baseWidget);
            return widget;
        }

        /// <summary>
        ///     Gets all the available widgets.
        /// </summary>
        public IEnumerable<Widget> GetAll()
        {
            if (this.cachedWidgets.None())
            {
                List<Type> widgetTypes = GetType().GetTypeInfo().Assembly.GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract)
                    .ToList();

                foreach (var widget in widgetTypes
                    .Where(t => !typeof(IUserDefinedWidget).IsAssignableFrom(t))
                    .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
                {
                    this.cachedWidgets.Add(widget.Category + widget.Name, widget);
                }
            }

            return this.cachedWidgets.Values;
        }

        /// <summary>
        ///     Removes the specified widget.
        /// </summary>
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
            var baseWidget = (Widget) widget;
            return baseWidget.Category + baseWidget.Name + widget.Id;
        }
    }
}