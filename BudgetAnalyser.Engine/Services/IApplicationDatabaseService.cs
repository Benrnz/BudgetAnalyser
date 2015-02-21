using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to access and manage the main Budget Analyser Data File.
    /// </summary>
    public interface IApplicationDatabaseService : IServiceFoundation
    {
        /// <summary>
        ///     Gets or sets a value indicating whether there are unsaved changes across all application data.
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        ///     Closes the currently loaded Budget Analyser file, and therefore any other application data is also closed.
        ///     Changes are discarded, no prompt or error will occur if there are unsaved changes. This check should be done before
        ///     calling this method.
        /// </summary>
        void Close();

        /// <summary>
        ///     Initialises the top level Application Database object that contains all information to retrieve data
        ///     for all other Budget Analyser datum.
        ///     This must be called first before other methods of this service can be used.
        /// </summary>
        ApplicationDatabase LoadPersistedStateData([NotNull] MainApplicationStateModelV1 storedState);

        /// <summary>
        ///     Notifies the service that data has changed and will need to be saved.
        /// </summary>
        void NotifyOfChange(ApplicationDataType dataType);

        /// <summary>
        ///     Prepares the persistent data for saving into permenant storage.
        /// </summary>
        MainApplicationStateModelV1 PreparePersistentStateData();

        /// <summary>
        ///     Saves all Budget Analyser application data.
        /// </summary>
        void Save();
    }
}