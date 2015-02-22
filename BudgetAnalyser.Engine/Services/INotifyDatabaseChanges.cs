using System;

namespace BudgetAnalyser.Engine.Services
{
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
        ///     Occurs when a new datasource has been loaded and is now available for use.
        /// </summary>
        event EventHandler NewDatasourceAvailable;
    }
}