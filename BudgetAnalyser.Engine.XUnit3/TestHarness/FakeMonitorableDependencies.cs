using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class FakeMonitorableDependencies : MonitorableDependencies
{
#nullable enable
    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitorableDependencies" /> class.
    /// </summary>
    public FakeMonitorableDependencies() : base(new LedgerCalculation(new FakeLogger()), new FakeLogger())
    {
        var availableDependencies = PrivateAccessor.GetField<MonitorableDependencies>(this, "availableDependencies") as IDictionary<Type, object?>;
        SupportedWidgetDependencyTypes = availableDependencies!.Select(kvp => kvp.Key).ToList();
    }

    /// <summary>
    ///     Gets a list of supported types
    /// </summary>
    public IEnumerable<Type> SupportedWidgetDependencyTypes
    {
        get;
        private set;
    }


    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>A boolean value indicating if the dependency has significantly changed, true if so, otherwise false.</returns>
    public override bool NotifyOfDependencyChange<T>(T? dependency) where T : class
    {
        return true;
    }

    public override object RetrieveDependency(Type key)
    {
        return null!;
    }

    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <param name="typeKey">The type key.</param>
    /// <returns>A boolean value indicating if the dependency has significantly change, true if so, otherwise false.</returns>
    protected override bool NotifyOfDependencyChange(object? dependency, Type typeKey)
    {
        return true;
    }
}
