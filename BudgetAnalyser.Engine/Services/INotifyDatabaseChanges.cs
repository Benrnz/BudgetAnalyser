namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     An interface to expose notifications for the UI Controllers so they can update when database changes have occured.
/// </summary>
public interface INotifyDatabaseChanges
{
    /// <summary>
    ///     Occurs when the underlying storage for transactions is closed.
    ///     This allows the UI to update and clear accordingly.
    ///     Opening and closing files is controlled centrally, not by this service.
    /// </summary>
    event EventHandler Closed;

    /// <summary>
    ///     Occurs when a new data source has been loaded and is now available for use.
    /// </summary>
    event EventHandler NewDataSourceAvailable;

    /// <summary>
    ///     Occurs when the service has finished saving data. This allows the controller to update any clientside view-models.
    /// </summary>
    event EventHandler Saved;

    /// <summary>
    ///     Occurs just before Saving the model. Can be used to request more information from the UI Controllers.
    /// </summary>
    event EventHandler<ValidatingEventArgs> Saving;

    /// <summary>
    ///     Occurs just before Validating the model.  Can be used to ensure the UI Controller has updated any necessary
    ///     information with its service.
    /// </summary>
    event EventHandler<ValidatingEventArgs> Validating;
}
