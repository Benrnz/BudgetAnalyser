namespace BudgetAnalyser.Engine.Services;

public interface IMonitorableDependencies
{
    /// <summary>
    /// An event that is raised when a dependency that is monitored by this class has been updated. Consumers can then call <see cref="RetrieveDependency"/> to fetch the updated dependency.
    /// </summary>
    event EventHandler<DependencyChangedEventArgs>? DependencyChanged;

    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>A boolean value indicating if the dependency has significantly changed, true if so, otherwise false.</returns>
    bool NotifyOfDependencyChange<T>(T? dependency);

    object RetrieveDependency(Type key);
}
