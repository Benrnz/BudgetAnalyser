using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A repository to get, create, and remove widgets
/// </summary>
public interface IWidgetRepository
{
    /// <summary>
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s. These can only be created after receiving the application state.
    /// </summary>
    /// <param name="widgetType">The full type name of the widget type.</param>
    /// <param name="id">A unique identifier for the instance</param>
    IUserDefinedWidget Create(string widgetType, string id);

    /// <summary>
    ///     Gets all the available widgets.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Preferred term in repository")]
    IEnumerable<Widget> GetAll();

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    void Remove(IUserDefinedWidget widget);
}
