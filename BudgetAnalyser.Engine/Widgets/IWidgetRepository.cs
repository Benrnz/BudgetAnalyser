namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A repository to get, create, and remove widgets
/// </summary>
public interface IWidgetRepository
{
    /// <summary>
    ///     Creates a new widget file with a default set of widgets and saves it.
    /// </summary>
    Task CreateNewAndSaveAsync(string storageKey);

    /// <summary>
    ///     Loads the widgets from the provided storage key (filename).
    /// </summary>
    Task<IEnumerable<Widget>> LoadAsync(string storageKey, bool isEncrypted);

    /// <summary>
    ///     Saves the widget collection to the provided storage key (filename).
    /// </summary>
    Task SaveAsync(IEnumerable<Widget> widgets, string storageKey, bool isEncrypted);
}
