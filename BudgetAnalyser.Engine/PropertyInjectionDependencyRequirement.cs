namespace BudgetAnalyser.Engine;

/// <summary>
///     Specifies that there are required property injections to perform for the application to function. This class is IoC container independent.
///     Property Injection is seldom used, but sometimes required for data binding in the UI to static properties.
/// </summary>
public class PropertyInjectionDependencyRequirement
{
    /// <summary>
    ///     Get or set the delegate that can assign a concrete dependency into a Property that requires it.
    /// </summary>
    public required Action<object> PropertyInjectionAssignment { get; init; }

    /// <summary>
    ///     Get or set a type describing the Type of the dependency required.
    /// </summary>
    public required Type Type { get; init; }
}
