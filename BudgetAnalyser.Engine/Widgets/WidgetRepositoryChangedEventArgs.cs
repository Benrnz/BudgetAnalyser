using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    public class WidgetRepositoryChangedEventArgs : EventArgs
    {
        public WidgetRepositoryChangedEventArgs(Widget widgetRemoved)
        {
            WidgetRemoved = widgetRemoved;
        }

        public Widget WidgetRemoved { get; private set; }
    }
}