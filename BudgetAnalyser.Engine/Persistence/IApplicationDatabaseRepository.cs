namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     This is the top level master repository that contains references to all other models.
/// </summary>
internal interface IApplicationDatabaseRepository
{
    /// <summary>
    ///     Creates a new budget analyser database graph.
    /// </summary>
    Task<ApplicationDatabase> CreateNewAsync(string storageKey);

    /// <summary>
    ///     Loads the Budget Analyser database graph from persistent storage.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">File does not exist.</exception>
    /// <exception cref="DataFormatException">Deserialisation Application Database file failed, an exception was thrown by the deserialiser, the file format is invalid.</exception>
    Task<ApplicationDatabase> LoadAsync(string storageKey);

    /// <summary>
    ///     Saves the Budget Analyser database graph to persistent storage.
    /// </summary>
    /// <param name="budgetAnalyserDatabase">The budget analyser database.</param>
    Task SaveAsync(ApplicationDatabase budgetAnalyserDatabase);
}
