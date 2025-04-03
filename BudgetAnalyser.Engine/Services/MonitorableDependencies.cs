using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A class that contains references to services that can be used as dependencies for the Dashboard Service.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class MonitorableDependencies : IMonitorableDependencies
{
    private readonly IDictionary<Type, object?> availableDependencies;
    private readonly Dictionary<Type, long> changesHashes = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitorableDependencies" /> class.
    /// </summary>
    public MonitorableDependencies(LedgerCalculation ledgerCalculator, ILogger logger)
    {
        this.availableDependencies = new Dictionary<Type, object?>
        {
            [typeof(StatementModel)] = null,
            [typeof(BudgetCollection)] = null,
            [typeof(IBudgetCurrencyContext)] = null,
            [typeof(LedgerBook)] = null,
            [typeof(IBudgetBucketRepository)] = null,
            [typeof(GlobalFilterCriteria)] = null,
            [typeof(LedgerCalculation)] = ledgerCalculator,
            [typeof(ApplicationDatabase)] = null,
            [typeof(ITransactionRuleService)] = null,
            [typeof(ILogger)] = logger,
            [typeof(IDirtyDataService)] = null,
        };
    }

    public event EventHandler<DependencyChangedEventArgs>? DependencyChanged;

    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <returns>A boolean value indicating if the dependency has significantly changed, true if so, otherwise false.</returns>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Preferred method of passing type parameter")]
    public virtual bool NotifyOfDependencyChange<T>(T? dependency)
    {
        return NotifyOfDependencyChange(dependency, typeof(T));
    }

    public virtual object RetrieveDependency(Type key)
    {
        if (!this.availableDependencies.TryGetValue(key, out var retrievedObject))
        {
            // If you get an exception here first check the Constructor for the types available.
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "The requested dependency {0} is not supported.", key.Name));
        }

        return retrievedObject!;
    }

    /// <summary>
    ///     Notifies this service of dependency that has changed.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <param name="typeKey">The type key.</param>
    /// <returns>A boolean value indicating if the dependency has significantly changed, true if so, otherwise false.</returns>
    protected virtual bool NotifyOfDependencyChange(object? dependency, Type typeKey)
    {
        if (dependency is null)
        {
            return false;
        }

        if (!this.availableDependencies.ContainsKey(typeKey))
        {
            throw new KeyNotFoundException($"The requested dependency {typeKey.Name} is not supported.");
        }

        this.availableDependencies[typeKey] = dependency;
        if (HasDependencySignificantlyChanged(dependency, typeKey))
        {
            DependencyChanged?.Invoke(this, new DependencyChangedEventArgs(typeKey));
            return true;
        }

        return false;
    }

    private bool HasDependencySignificantlyChanged(object dependency, Type typeKey)
    {
        if (dependency is not IDataChangeDetection supportsDataChangeDetection)
        {
            // Dependency doesn't support change hashes so every change is deemed worthy to trigger an update the UI.
            return true;
        }

        var newHash = supportsDataChangeDetection.SignificantDataChangeHash();
        if (!this.changesHashes.TryGetValue(typeKey, out var hash))
        {
            this.changesHashes.Add(typeKey, newHash);
            return true;
        }

        var result = hash != newHash;
        this.changesHashes[typeKey] = newHash;
        return result;
    }
}
