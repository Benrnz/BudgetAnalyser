namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Lists, and finds available widgets within the application.
/// </summary>
public interface IStandardWidgetCatalog
{
    /// <summary>
    ///     Checks to see if a widget exists and returns it if so, otherwise returns null.
    /// </summary>
    /// <param name="fullTypeName">The full typename with namespace</param>
    Widget? FindWidget(string fullTypeName);

    /// <summary>
    ///     Returns a full catalogue of all available widgets. This is sorted and keyed by the Category and Name of the widget.
    ///     Only one instance of user defined widgets are returned.
    /// </summary>
    IEnumerable<Widget> GetAll();
}
