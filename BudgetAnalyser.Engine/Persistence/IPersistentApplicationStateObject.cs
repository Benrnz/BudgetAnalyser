namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     An interface that represents an object that needs to be persisted with application state when the application shuts
///     down. This is not model data. For example, it is windows size, last loaded file, user's UI preferences.
/// </summary>
public interface IPersistentApplicationStateObject
{
    /// <summary>
    ///     Gets the order in which this object should be loaded.
    /// </summary>
    int LoadSequence { get; }
}
