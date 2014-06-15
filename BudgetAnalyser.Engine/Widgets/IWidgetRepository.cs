using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    public interface IWidgetRepository
    {
        /// <summary>
        ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IMultiInstanceWidget" />s.
        ///     These can only be created after receiving the application state.
        /// </summary>
        /// <param name="widgetType">The full type name of the widget type.</param>
        /// <param name="id">A unique identifier for the instance</param>
        IMultiInstanceWidget Create(string widgetType, string id);

        event EventHandler<WidgetRepositoryChangedEventArgs> WidgetRemoved;

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Prefered term in repository")]
        IEnumerable<Widget> GetAll();

        void Remove(IMultiInstanceWidget widget);
    }
}