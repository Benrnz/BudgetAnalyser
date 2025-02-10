namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Widgets that implement this interface, will have multiple instances created to show different statuses. Each instance must be uniquely identified by the <see cref="Id" /> property
///     combined with the <see cref="WidgetType" /> Property.
/// </summary>
public interface IUserDefinedWidget
{
    /// <summary>
    ///     Gets or sets a unique identifier for the widget. This is required for persistence purposes.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="IUserDefinedWidget" /> is visible. Ie: It has been hidden by the user.
    /// </summary>
    bool Visibility { get; set; }

    /// <summary>
    ///     Gets the type of the widget. Optionally allows the implementation to override the widget type description used in the user interface.
    /// </summary>
    Type WidgetType { get; }
}
