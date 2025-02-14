namespace BudgetAnalyser.Engine.Services;

public interface IMonitorableDependencies
{
    event EventHandler<DependencyChangedEventArgs>? DependencyChanged;

    /// <summary>
    ///     Gets a list of supported types
    /// </summary>
    IEnumerable<Type> SupportedWidgetDependencyTypes { get; }

    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>A boolean value indicating if the dependency has significantly changed, true if so, otherwise false.</returns>
    bool NotifyOfDependencyChange<T>(T? dependency);

    object RetrieveDependency(Type key);
}
