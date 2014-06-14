using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Widgets
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReflectionWidgetRepository : IWidgetRepository
    {
        private static IEnumerable<Widget> CachedWidgets;

        public IEnumerable<Widget> GetAll()
        {
            if (CachedWidgets == null)
            {
                IEnumerable<Type> widgetTypes = GetType().Assembly.GetExportedTypes()
                    .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.IsAbstract);
                CachedWidgets = widgetTypes.Select(widgetType => Activator.CreateInstance(widgetType) as Widget)
                    .OrderBy(w => w.Category)
                    .ThenBy(w => w.Name)
                    .ToList();
            }

            return CachedWidgets;
        }
    }
}