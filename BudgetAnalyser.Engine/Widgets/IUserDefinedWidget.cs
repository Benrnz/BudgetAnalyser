namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Widgets that implement this interface, will have multiple instances created to show different statuses. Each instance must be uniquely identified by the <see cref="Id" /> property.
/// </summary>
public interface IUserDefinedWidget
{
    /// <summary>
    ///     Gets or sets a unique identifier for the widget. This is required for persistence purposes.
    /// </summary>
    string Id { get; set; }
}
