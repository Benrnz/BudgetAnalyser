using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Widget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReflectionWidgetRepository : IWidgetRepository
    {
        public IEnumerable<Widget> GetAll()
        {
            var widgetTypes = GetType().Assembly.GetExportedTypes()
                .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract);
            return widgetTypes.Select(widgetType => Activator.CreateInstance(widgetType) as Widget)
                .OrderBy(w => w.Category)
                .ThenBy(w => w.Name)
                .ToList();
        }
    }
}