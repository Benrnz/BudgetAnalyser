using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A repository for widgets based on reflecting across available types in the attached assemblies.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.IWidgetRepository" />
[AutoRegisterWithIoC(SingleInstance = true)]
[UsedImplicitly] // Used by IoC
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
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s. These can only be created after receiving the application state.
    /// </summary>
    /// <param name="widgetType">The full type name of the widget type.</param>
    /// <param name="id">A unique identifier for the instance</param>
    /// <exception cref="System.NotSupportedException">
    ///     The widget type specified  + widgetType +  is not found in any known type library. Or the widget type specified  + widgetType +  is not a IUserDefinedWidget
    /// </exception>
    /// <exception cref="System.ArgumentException">A widget with this key already exists.</exception>
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IUserDefinedWidget")]
    public IUserDefinedWidget Create(string widgetType, string id)
    {
        var type = Type.GetType(widgetType) ?? throw new NotSupportedException($"The widget type specified {widgetType} is not found in any known type library.");
        if (!typeof(IUserDefinedWidget).IsAssignableFrom(type))
        {
            throw new NotSupportedException($"The widget type specified {widgetType} is not a IUserDefinedWidget");
        }

        var widget = Activator.CreateInstance(type) as IUserDefinedWidget;
        Debug.Assert(widget is not null);
        widget.Id = id;
        var key = BuildMultiUseWidgetKey(widget);

        if (this.cachedWidgets.ContainsKey(key))
        {
            throw new ArgumentException("A widget with this key already exists.", nameof(id));
        }

        var baseWidget = (Widget)widget;
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
            var widgetTypes = GetType().GetTypeInfo().Assembly.GetExportedTypes()
                .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract)
                .ToList();

            foreach (var widget in widgetTypes
                         .Where(t => !typeof(IUserDefinedWidget).IsAssignableFrom(t))
                         .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
            {
                var w = widget ?? throw new NotSupportedException("Widget could not be created.");
                this.cachedWidgets.Add(w.Category + w.Name, w);
            }
        }

        return this.cachedWidgets.Values;
    }

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    public void Remove(IUserDefinedWidget widget)
    {
        this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget));
    }

    private static string BuildMultiUseWidgetKey(IUserDefinedWidget widget)
    {
        var baseWidget = (Widget)widget;
        return baseWidget.Category + baseWidget.Name + widget.Id;
    }
}
