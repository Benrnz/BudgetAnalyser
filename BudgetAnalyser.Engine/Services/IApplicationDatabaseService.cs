using System.Text;
using System.Threading.Tasks;
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
        ///     Loads the specified Budget Analyser file by file name. This will also trigger a load on all subordinate
        ///     data contained within and referenced by the top level application database.
        ///     No warning will be given if there is any unsaved data. This should be checked before calling this method.
        /// </summary>
        /// <param name="storageKey">Name and path to the file.</param>
        Task<ApplicationDatabase> LoadAsync([NotNull] string storageKey);

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
        /// <exception cref="ValidationWarningException">Will be thrown if there are any validation errors.</exception>
        Task SaveAsync();

        /// <summary>
        ///     Validates all models in the application database.
        ///     This method does not use <see cref="ValidationWarningException" /> to return any validation messages.
        /// </summary>
        /// <param name="messages">Will append any validation messages found.</param>
        /// <returns>True if valid, otherwise false.</returns>
        bool ValidateAll(StringBuilder messages);
    }
}