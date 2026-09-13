namespace BudgetAnalyser.Engine.Services;

public interface IDirtyDataService
{
    /// <summary>
    ///     Returns true if there are any unsaved changes in the application.
    /// </summary>
    bool HasUnsavedChanges { get; }

    /// <summary>
    ///     Reset all dirty data flags to clean/saved (false).
    /// </summary>
    void ClearAllDirtyDataFlags();

    /// <summary>
    ///     Is a specific type of data dirty
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    bool IsDirty(ApplicationDataType dataType);

    /// <summary>
    ///     Notifies the service that data has changed and a dirty flag should be set to true.
    /// </summary>
    /// <param name="dataType"></param>
    void NotifyOfChange(ApplicationDataType dataType);

    /// <summary>
    ///     Set all dirty data flags to dirty requiring a save. Useful to force a fresh save of everything.
    /// </summary>
    void SetAllDirtyFlags();
}
