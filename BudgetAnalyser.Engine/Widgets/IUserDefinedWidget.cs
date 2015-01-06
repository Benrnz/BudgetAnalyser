using System;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Widgets that implement this interface, will have multiple instances created to show different statuses.  
    ///     Each instance must be uniquely identified by the <see cref="Id" /> property combined with the <see cref="WidgetType" /> Property.
    /// </summary>
    public interface IUserDefinedWidget
    {
        string Id { get; set; }

        bool Visibility { get; set; }

        Type WidgetType { get; }

        void Initialise(MultiInstanceWidgetState state);
    }
}