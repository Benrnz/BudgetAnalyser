using System.Reflection;

namespace BudgetAnalyser.Engine.Widgets;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once UnusedType.Global // IoC
internal class WidgetCatalog : IStandardWidgetCatalog
{
    private readonly List<Widget> cachedWidgets = new();

    public WidgetCatalog()
    {
        var everyConcreteWidget = GetType().GetTypeInfo().Assembly.GetExportedTypes()
            .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract)
            .ToList();

        // Find and instantiate all standard widgets (not user defined widgets)
        foreach (var widget in everyConcreteWidget
                     .Where(t => !typeof(IUserDefinedWidget).IsAssignableFrom(t))
                     .Select(widgetType => Activator.CreateInstance(widgetType) as Widget))
        {
            var w = widget ?? throw new DataFormatException("Widget could not be created.");
            this.cachedWidgets.Add(w);
        }
    }

    /// <inheritdoc />
    public Widget? FindWidget(string fullTypeName)
    {
        return this.cachedWidgets.FirstOrDefault(w => w.GetType().FullName! == fullTypeName);
    }

    /// <inheritdoc />
    public IEnumerable<Widget> GetAll()
    {
        return this.cachedWidgets;
    }
}
